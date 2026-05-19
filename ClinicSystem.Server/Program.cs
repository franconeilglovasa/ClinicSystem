using System.Text;
using ClinicSystem.Server.Data;
using ClinicSystem.Server.Models;
using ClinicSystem.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClinicSystem.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

            builder.Services.AddAuthorization();

            // CORS for Angular dev
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AngularDev", policy =>
                    policy.WithOrigins("https://localhost:56699", "http://localhost:56699")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            // Application services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddHttpClient<IOllamaService, OllamaService>();

            // Controllers & OpenAPI
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // File upload size limit (20 MB)
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseCors("AngularDev");
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedDatabase(db, userManager, roleManager);
            }

            await app.RunAsync();
        }

        private static async Task SeedDatabase(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await db.Database.MigrateAsync();

            // Seed roles
            foreach (var role in Enum.GetNames<UserRole>())
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed default admin user
            const string adminEmail = "admin@clinicsystem.com";
            const string adminPassword = "Admin@123";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                    return;
            }

            // Keep the default admin account healthy even across re-runs.
            var needsUpdate = false;
            if (!admin.IsActive)
            {
                admin.IsActive = true;
                needsUpdate = true;
            }
            if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                needsUpdate = true;
            }
            if (admin.Role != UserRole.Admin)
            {
                admin.Role = UserRole.Admin;
                needsUpdate = true;
            }
            if (needsUpdate)
                await userManager.UpdateAsync(admin);

            if (!await userManager.IsInRoleAsync(admin, UserRole.Admin.ToString()))
                await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());

            await SeedSampleData(db, userManager);
        }

        private static async Task SeedSampleData(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            // Always ensure core non-admin users exist for demos.
            var doctor = await EnsureUserAsync(
                userManager,
                "doctor@clinicsystem.com",
                "Doctor@123",
                "Dr. Maria Santos",
                UserRole.Doctor,
                specialty: "Internal Medicine",
                licenseNumber: "DOC-10234"
            );

            var nurse = await EnsureUserAsync(
                userManager,
                "nurse@clinicsystem.com",
                "Nurse@123",
                "Nurse John Rivera",
                UserRole.Nurse
            );

            var labTech = await EnsureUserAsync(
                userManager,
                "lab@clinicsystem.com",
                "Lab@123",
                "Lab Tech Carla Dizon",
                UserRole.Laboratory
            );

            var cashier = await EnsureUserAsync(
                userManager,
                "cashier@clinicsystem.com",
                "Cashier@123",
                "Cashier Anne Cruz",
                UserRole.Cashier
            );

            // Keep seed idempotent: if patients exist, do not duplicate sample records.
            if (await db.Patients.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            var patient1 = new Patient
            {
                FirstName = "Juan",
                LastName = "Dela Cruz",
                DateOfBirth = new DateTime(1988, 3, 14),
                Gender = Gender.Male,
                ContactNumber = "09171234567",
                Address = "Makati City",
                Email = "juan.delacruz@example.com",
                CreatedAt = now.AddDays(-3)
            };

            var patient2 = new Patient
            {
                FirstName = "Maria",
                LastName = "Reyes",
                DateOfBirth = new DateTime(1995, 9, 25),
                Gender = Gender.Female,
                ContactNumber = "09179876543",
                Address = "Quezon City",
                Email = "maria.reyes@example.com",
                CreatedAt = now.AddDays(-2)
            };

            var patient3 = new Patient
            {
                FirstName = "Alex",
                LastName = "Garcia",
                DateOfBirth = new DateTime(2001, 11, 2),
                Gender = Gender.Other,
                ContactNumber = "09175551234",
                Address = "Pasig City",
                Email = "alex.garcia@example.com",
                CreatedAt = now.AddDays(-1)
            };

            db.Patients.AddRange(patient1, patient2, patient3);

            var visit1 = new Visit
            {
                PatientId = patient1.PatientId,
                VisitDate = now.AddHours(-6),
                Status = VisitStatus.ForBilling,
                ChiefComplaint = "Persistent cough and low-grade fever",
                NurseId = nurse.Id,
                DoctorId = doctor.Id,
                CreatedAt = now.AddHours(-6)
            };

            var visit2 = new Visit
            {
                PatientId = patient2.PatientId,
                VisitDate = now.AddHours(-2),
                Status = VisitStatus.Waiting,
                ChiefComplaint = "Headache and fatigue",
                CreatedAt = now.AddHours(-2)
            };

            db.Visits.AddRange(visit1, visit2);

            db.Vitals.Add(new Vitals
            {
                VisitId = visit1.VisitId,
                BloodPressure = "130/85",
                HeartRate = 88,
                Temperature = 37.8m,
                Weight = 72.5m,
                Height = 170m,
                OxygenSaturation = 97,
                RespiratoryRate = 18,
                Notes = "Patient appears mildly distressed but oriented.",
                RecordedByNurseId = nurse.Id,
                RecordedAt = now.AddHours(-5)
            });

            db.PatientHistories.Add(new PatientHistory
            {
                VisitId = visit1.VisitId,
                PatientId = patient1.PatientId,
                ChiefComplaint = "Persistent cough and low-grade fever",
                PresentIllness = "Cough for 5 days with intermittent fever, no dyspnea.",
                PastMedicalHistory = "No known chronic illnesses.",
                FamilyHistory = "Father with hypertension.",
                SocialHistory = "Non-smoker, occasional alcohol use.",
                Allergies = "No known drug allergies.",
                CurrentMedications = "Paracetamol as needed.",
                ReviewOfSystems = "Negative for chest pain and palpitations.",
                PhysicalExamination = "Mild pharyngeal erythema; lungs with coarse breath sounds.",
                Assessment = "Acute bronchitis, likely viral etiology.",
                Plan = "Symptomatic treatment, hydration, follow-up in 3 days.",
                DoctorId = doctor.Id,
                CreatedAt = now.AddHours(-4),
                UpdatedAt = now.AddHours(-4)
            });

            var prescription = new Prescription
            {
                VisitId = visit1.VisitId,
                PatientId = patient1.PatientId,
                DoctorId = doctor.Id,
                Date = now.AddHours(-4),
                Instructions = "Complete full course. Return if fever worsens.",
                CreatedAt = now.AddHours(-4)
            };

            var rxItems = new List<PrescriptionItem>
            {
                new()
                {
                    PrescriptionId = prescription.PrescriptionId,
                    Medication = "Amoxicillin",
                    Dosage = "500 mg",
                    Frequency = "Every 8 hours",
                    Duration = "7 days",
                    Instructions = "Take after meals"
                },
                new()
                {
                    PrescriptionId = prescription.PrescriptionId,
                    Medication = "Paracetamol",
                    Dosage = "500 mg",
                    Frequency = "Every 6 hours as needed",
                    Duration = "3 days",
                    Instructions = "For fever above 38C"
                }
            };

            db.Prescriptions.Add(prescription);
            db.PrescriptionItems.AddRange(rxItems);

            var labRequest = new LabRequest
            {
                VisitId = visit1.VisitId,
                PatientId = patient1.PatientId,
                RequestedByDoctorId = doctor.Id,
                TestType = "Hematology",
                TestName = "Complete Blood Count",
                Notes = "Rule out bacterial infection.",
                Status = LabRequestStatus.Completed,
                RequestedAt = now.AddHours(-4)
            };

            var labResult = new LabResult
            {
                RequestId = labRequest.RequestId,
                LabTechId = labTech.Id,
                Findings = "Mild leukocytosis.",
                Notes = "WBC 11.8 x10^9/L; neutrophil predominance.",
                ResultDate = now.AddHours(-3),
                CreatedAt = now.AddHours(-3)
            };

            db.LabRequests.Add(labRequest);
            db.LabResults.Add(labResult);

            var bill = new Bill
            {
                VisitId = visit1.VisitId,
                PatientId = patient1.PatientId,
                CashierId = cashier.Id,
                TotalAmount = 1400m,
                PaidAmount = 500m,
                Status = BillStatus.PartiallyPaid,
                Notes = "Initial payment received.",
                CreatedAt = now.AddHours(-2),
                PaidAt = now.AddHours(-2)
            };

            var billItems = new List<BillItem>
            {
                new()
                {
                    BillId = bill.BillId,
                    Description = "Doctor Consultation",
                    Category = BillItemCategory.Consultation,
                    UnitPrice = 800m,
                    Quantity = 1
                },
                new()
                {
                    BillId = bill.BillId,
                    Description = "CBC Laboratory Test",
                    Category = BillItemCategory.Laboratory,
                    UnitPrice = 600m,
                    Quantity = 1
                }
            };

            db.Bills.Add(bill);
            db.BillItems.AddRange(billItems);

            db.AISuggestions.Add(new AISuggestion
            {
                VisitId = visit1.VisitId,
                PatientId = patient1.PatientId,
                PromptContext = "Seed sample context",
                Response = "Sample AI suggestion: Consider acute bronchitis management, monitor fever curve, and reassess if symptoms persist beyond 72 hours.",
                Model = "llama3",
                RequestedByDoctorId = doctor.Id,
                GeneratedAt = now.AddHours(-3)
            });

            await db.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            UserRole role,
            string? specialty = null,
            string? licenseNumber = null)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    Specialty = specialty,
                    LicenseNumber = licenseNumber,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                    return user;
            }

            var needsUpdate = false;
            if (!user.IsActive)
            {
                user.IsActive = true;
                needsUpdate = true;
            }
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                needsUpdate = true;
            }
            if (user.Role != role)
            {
                user.Role = role;
                needsUpdate = true;
            }
            if (user.FullName != fullName)
            {
                user.FullName = fullName;
                needsUpdate = true;
            }
            if (specialty != null && user.Specialty != specialty)
            {
                user.Specialty = specialty;
                needsUpdate = true;
            }
            if (licenseNumber != null && user.LicenseNumber != licenseNumber)
            {
                user.LicenseNumber = licenseNumber;
                needsUpdate = true;
            }

            if (needsUpdate)
                await userManager.UpdateAsync(user);

            if (!await userManager.IsInRoleAsync(user, role.ToString()))
                await userManager.AddToRoleAsync(user, role.ToString());

            return user;
        }
    }
}

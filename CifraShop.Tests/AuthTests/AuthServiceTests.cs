using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CifraShop.Tests.AuthTests
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        // Инициализация JWT-конфигурацию через in-memory коллекцию и создание экземпляра AuthService перед каждым тестом.
        public AuthServiceTests()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "TestSecretKey_Minimum32Characters_Long!!" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpiryMinutes", "60" }
                })
                .Build();
            _authService = new AuthService(_config);
        }
        // Создание экземпляра AuthService с тестовой JWT-конфигурацией, с возможностью  переопределить отдельные параметры.
        private static AuthService CreatinganAuthServiceInstanceWithATestJWTConfiguration(Dictionary<string, string?> overrides)
        {
            var defaults = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKey_Minimum32Characters_Long!!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryMinutes", "60" }
            };
            foreach (var kv in overrides)
                defaults[kv.Key] = kv.Value;
            var config = new ConfigurationBuilder().AddInMemoryCollection(defaults).Build();
            return new AuthService(config);
        }

        // Статический экземпляр обработчика JWT-токенов, используемый в тестах.
        private static readonly System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler TokenHandler = new();

        // Создание тестового пользователя с заданными параметрами или значениями по умолчанию.
        private static User CreatingATestUserWithSpecifiedParameters(int id = 1, string email = "a@b.com", UserRole role = UserRole.Student, string? branch = null)
            => new() { Id = id, Email = email, Password = "hashed", Balance = 0, Role = role, Branch = branch };

        // Проверка, что GenerateToken возвращает непустой JWT-токен, содержащий точки-разделители.
        [Fact]
        public void CheckingThatGenerateTokenReturnsANonEmptyJWTtoken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters());

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token);
        }

        // Проверка, что сгенерированный JWT-токен успешно читается стандартным обработчиком.
        [Fact]
        public void CheckingThatTheGeneratedJWTtokenIsSuccessfullyRead()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters());

            Assert.True(TokenHandler.CanReadToken(token));
        }

        [Fact]
        // Проверка, что сгенерированный JWT-токен содержит  NameIdentifier со значением 42.
        public void CheckingThatTheGeneratedJWTTokenContainsTheNameIdentifierClaim()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 42));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("42", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }

        [Fact]
        //Проверка, что сгенерированный JWT - токен содержит  Email со значением "admin@email.com".
        public void CheckingThatTheGeneratedJWTTokenContainsTheEmailClaim()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(email: "admin@email.com"));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("admin@email.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        }

        [Fact]
        // Проверка, что сгенерированный JWT-токен содержит  Role со значением "Student".
        public void ChecksThatTheGeneratedJWTTokenContainsAClaimRoleWithTheValueStudent()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(role: UserRole.Student));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Student", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        //Проверка что сгенерированный JWT-токен содержит  Role со значением админ
        public void ChecksThatTheGeneraatedJWTTokenContainsAClaimRoleTheValueAdmin()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(role: UserRole.Admin));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        // Проверка, что сгенерированный JWT-токен содержит  branch со значением "Филиал 1".
        public void CheckingThatTheGeneratedJWTTokenContainsTheClaimBranch()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: "Филиал 1"));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Филиал 1", jwt.Claims.First(c => c.Type == "branch").Value);
        }

        [Fact]
        // Проверка, что при отсутствии филиала  соответствующий claim не добавляется в токен.
        public void ChecksThatIfThereIsNoBranchTheCorrespondingClaimisNotAddedToTheToken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: null));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // Проверка, что при пустой строке филиала  branch не добавляется в токен.
        public void ChecksThatTheBranchIsNotAddedToTheTokenWhenTheBranchIsEmpty()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: ""));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // Проверка, что если филиал состоит только из пробелов, branch не добавляется в токен.
        public void CheckingThatIfTheBranchConsistsOnlyOfSpacesTheBranchIsNotAddedToTheToken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: "   "));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // Проверка, что токен содержит корректного издателя, заданного в конфигурации.
        public void CheckingThatTheTokenContainsAValidIssuer()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("TestIssuer", jwt.Issuer);
        }

        [Fact]
        // Проверка, что токен содержит корректную аудиторию, заданную в конфигурации.
        public void CheckingThatTheTokenContainsTheCorrectAudience()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("TestAudience", jwt.Audiences.First());
        }

        [Fact]
        // Проверка, что время истечения токена больше текущего и не превышает значение заданное в конфигурации (60 минут).
        public void CheckingThatTheTokensExpirationTimeIsGreaterThanTheCurrentTimeAndDoesNotExceedTheValue()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(61));
        }

        [Fact]
        // Проверка, что JWT-токен подписан алгоритмом HMAC SHA256 (HS256).
        public void CheckingThatTheTokenIsSignedWithTheHMACSHA256Algorithm()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("HS256", jwt.Header.Alg);
        }

        [Fact]
        // Проверка, что разные пользователи получают разные JWT-токены.
        public void ItChecksThatD0ifferentUsersReceiveDifferentTokens()
        {
            var token1 = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 1, email: "a@b.com"));
            var token2 = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 2, email: "c@d.com"));

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        // Проверка, что повторная генерация токена для одного и того же пользователя создаёт валидные JWT-токены
        public void CheckingThatRegeneratingTheTokenCreatesValidTokens()
        {
            var u = CreatingATestUserWithSpecifiedParameters();
            var token1 = _authService.GenerateToken(u);
            var token2 = _authService.GenerateToken(u);

            Assert.True(TokenHandler.CanReadToken(token1));
            Assert.True(TokenHandler.CanReadToken(token2));
        }

        [Fact]
        // Проверка, что даже при большом значении идентификатора (999999)  NameIdentifier сохраняется корректно.
        public void CheckingThatEvenWhenTheNameIdentifierValueIsLargeItIsStoredCorrectly()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 999999));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("999999", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }

        [Fact]
        // Проверка, что при отсутствии Issuer в конфигурации используется значение по умолчанию "CifraShop".
        public void CheckingThatTheDefaultValueIsUsedWhenTheIssuerIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Issuer", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("CifraShop", jwt.Issuer);
        }

        [Fact]
        // Проверка, что при отсутствии Audience в конфигурации используется значение по умолчанию "CifraShop".
        public void CheckingThatTheDefaultValueIsUsedWhenAudienceIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Audience", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("CifraShop", jwt.Audiences.First());
        }

        [Fact]
        // Проверка, что при отсутствии ExpiryMinutes в конфигурации используется значение по умолчанию 1440 минут (сутки).
        public void CheckingThatTheDefaultValueIsusedWhenExpiryMinutesIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:ExpiryMinutes", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(1439));
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(1441));
        }

        [Fact]
        // Проверка, что при отсутствии ключа (Jwt:Key) в конфигурации выбрасывается исключение InvalidOperationException.
        public void CheckingThatAnExceptionIsThrownIfNoKeyIsPresent()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Key", null } });

            Assert.Throws<InvalidOperationException>(() => svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));
        }

        [Fact]
        // Проверка, что при установке времени жизни токена 5 минут, он истекает в пределах ожидаемого  (4–6 минут).
        public void CheckingThatWhenTheTokensLifetimeIsSetItExpiresWithinTheExpectedTime()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:ExpiryMinutes", "5" } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(4));
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(6));
        }

        [Fact]
        // Проверка, что хэширование одного и того же пароля всегда даёт одинаковый результат.
        public void ChecksThatPasswordHashingAlwaysProducesTheSameResult()
        {
            var hash1 = _authService.HashPassword("password123");
            var hash2 = _authService.HashPassword("password123");

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        // Проверка, что разные пароли дают разные хэши.
        public void CheckingThatDifferentPasswordsProduceDifferentHashes()
        {
            var hash1 = _authService.HashPassword("password123");
            var hash2 = _authService.HashPassword("password456");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        // Проверка, что HashPassword возвращает корректную Base64-строку длиной 32 байта (SHA256).
        public void ChecksThatHashPasswordReturnsACorrectString()
        {
            var hash = _authService.HashPassword("test");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            var bytes = Convert.FromBase64String(hash);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        // Проверка, что хэширование пустой строки возвращает корректный непустой хэш.
        public void CheckingThatHashingAnEmptyStringReturnsAValidHash()
        {
            var hash = _authService.HashPassword("");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        // Проверка, что хэширование строки с Unicode (кириллица) возвращает непустой хэш.
        public void CheckingThatHashingAUnicodeStringReturnsANonEmptyHash()
        {
            var hash = _authService.HashPassword("пароль123");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        // Проверка, что хэширование очень длинной строки (10000 символов) возвращает корректный хэш длиной 32 байта.
        public void Checking10kCharHashLength()
        {
            var longPassword = new string('a', 10000);
            var hash = _authService.HashPassword(longPassword);

            Assert.NotNull(hash);
            Assert.Equal(32, Convert.FromBase64String(hash).Length);
        }

        [Fact]
        // Проверка, что хэш не содержит исходного пароля в открытом виде.
        public void CheckingHashExcludesPlaintextPassword()
        {
            var password = "secret123";
            var hash = _authService.HashPassword(password);

            Assert.DoesNotContain(password, hash);
        }

        [Fact]
        // Проверка, что VerifyPassword возвращает true для корректного пароля.
        public void CheckingCorrectPasswordReturnsTrue()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.True(_authService.VerifyPassword("secret123", hash));
        }

        [Fact]
        // Проверка, что VerifyPassword возвращает false для неверного пароля.
        public void CheckingWrongPasswordReturnsFalse()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.False(_authService.VerifyPassword("wrong", hash));
        }

        [Fact]
        // Проверка, что VerifyPassword возвращает false для пустого пароля.
        public void CheckingEmptyPasswordReturnsFalse()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.False(_authService.VerifyPassword("", hash));
        }

        [Fact]
        // Проверка, что VerifyPassword чувствительна к регистру: пароли "password" и "PASSWORD" не проходят проверку.
        public void CheckingCaseSensitivityRejectsWrongCaseVariants()
        {
            var hash = _authService.HashPassword("Password");

            Assert.False(_authService.VerifyPassword("password", hash));
            Assert.False(_authService.VerifyPassword("PASSWORD", hash));
        }

        [Fact]
        // Проверка, что VerifyPassword возвращает false при использовании хэша от другого пароля.
        public void CheckingWrongHashReturnsFalse()
        {
            var otherHash = _authService.HashPassword("other");

            Assert.False(_authService.VerifyPassword("secret123", otherHash));
        }

        [Fact]
        // Проверка, что VerifyPassword возвращает false при пустом хэше.
        public void CheckingEmptyHashReturnsFalse()
        {
            Assert.False(_authService.VerifyPassword("secret123", ""));
        }

        [Fact]
        // Проверка, что VerifyPassword корректно работает с Unicode-паролями (кириллица)
        public void CheckingUnicodePasswordVerification()
        {
            var hash = _authService.HashPassword("пароль");

            Assert.True(_authService.VerifyPassword("пароль", hash));
            Assert.False(_authService.VerifyPassword("пароль2", hash));
        }
    }
}

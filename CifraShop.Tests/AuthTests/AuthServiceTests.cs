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

        // РРЅРёС†РёР°Р»РёР·Р°С†РёСЏ JWT-РєРѕРЅС„РёРіСѓСЂР°С†РёСЋ С‡РµСЂРµР· in-memory РєРѕР»Р»РµРєС†РёСЋ Рё СЃРѕР·РґР°РЅРёРµ СЌРєР·РµРјРїР»СЏСЂР° AuthService РїРµСЂРµРґ РєР°Р¶РґС‹Рј С‚РµСЃС‚РѕРј.
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
        // РЎРѕР·РґР°РЅРёРµ СЌРєР·РµРјРїР»СЏСЂР° AuthService СЃ С‚РµСЃС‚РѕРІРѕР№ JWT-РєРѕРЅС„РёРіСѓСЂР°С†РёРµР№, СЃ РІРѕР·РјРѕР¶РЅРѕСЃС‚СЊСЋ  РїРµСЂРµРѕРїСЂРµРґРµР»РёС‚СЊ РѕС‚РґРµР»СЊРЅС‹Рµ РїР°СЂР°РјРµС‚СЂС‹.
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

        // РЎС‚Р°С‚РёС‡РµСЃРєРёР№ СЌРєР·РµРјРїР»СЏСЂ РѕР±СЂР°Р±РѕС‚С‡РёРєР° JWT-С‚РѕРєРµРЅРѕРІ, РёСЃРїРѕР»СЊР·СѓРµРјС‹Р№ РІ С‚РµСЃС‚Р°С….
        private static readonly System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler TokenHandler = new();

        // РЎРѕР·РґР°РЅРёРµ С‚РµСЃС‚РѕРІРѕРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ СЃ Р·Р°РґР°РЅРЅС‹РјРё РїР°СЂР°РјРµС‚СЂР°РјРё РёР»Рё Р·РЅР°С‡РµРЅРёСЏРјРё РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ.
        private static User CreatingATestUserWithSpecifiedParameters(int id = 1, string email = "a@b.com", UserRole role = UserRole.Student, string? branch = null)
            => new() { Id = id, Email = email, Password = "hashed", Balance = 0, Role = role, Branch = branch };

        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GenerateToken РІРѕР·РІСЂР°С‰Р°РµС‚ РЅРµРїСѓСЃС‚РѕР№ JWT-С‚РѕРєРµРЅ, СЃРѕРґРµСЂР¶Р°С‰РёР№ С‚РѕС‡РєРё-СЂР°Р·РґРµР»РёС‚РµР»Рё.
        [Fact]
        public void CheckingThatGenerateTokenReturnsANonEmptyJWTtoken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters());

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token);
        }

        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT-С‚РѕРєРµРЅ СѓСЃРїРµС€РЅРѕ С‡РёС‚Р°РµС‚СЃСЏ СЃС‚Р°РЅРґР°СЂС‚РЅС‹Рј РѕР±СЂР°Р±РѕС‚С‡РёРєРѕРј.
        [Fact]
        public void CheckingThatTheGeneratedJWTtokenIsSuccessfullyRead()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters());

            Assert.True(TokenHandler.CanReadToken(token));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT-С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚  NameIdentifier СЃРѕ Р·РЅР°С‡РµРЅРёРµРј 42.
        public void CheckingThatTheGeneratedJWTTokenContainsTheNameIdentifierClaim()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 42));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("42", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }

        [Fact]
        //РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT - С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚  Email СЃРѕ Р·РЅР°С‡РµРЅРёРµРј "admin@email.com".
        public void CheckingThatTheGeneratedJWTTokenContainsTheEmailClaim()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(email: "admin@email.com"));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("admin@email.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT-С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚  Role СЃРѕ Р·РЅР°С‡РµРЅРёРµРј "Student".
        public void ChecksThatTheGeneratedJWTTokenContainsAClaimRoleWithTheValueStudent()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(role: UserRole.Student));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Student", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        //РџСЂРѕРІРµСЂРєР° С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT-С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚  Role СЃРѕ Р·РЅР°С‡РµРЅРёРµРј Р°РґРјРёРЅ
        public void ChecksThatTheGeneraatedJWTTokenContainsAClaimRoleTheValueAdmin()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(role: UserRole.Admin));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРіРµРЅРµСЂРёСЂРѕРІР°РЅРЅС‹Р№ JWT-С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚  branch СЃРѕ Р·РЅР°С‡РµРЅРёРµРј "Р¤РёР»РёР°Р» 1".
        public void CheckingThatTheGeneratedJWTTokenContainsTheClaimBranch()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: "Р¤РёР»РёР°Р» 1"));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("Р¤РёР»РёР°Р» 1", jwt.Claims.First(c => c.Type == "branch").Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё С„РёР»РёР°Р»Р°  СЃРѕРѕС‚РІРµС‚СЃС‚РІСѓСЋС‰РёР№ claim РЅРµ РґРѕР±Р°РІР»СЏРµС‚СЃСЏ РІ С‚РѕРєРµРЅ.
        public void ChecksThatIfThereIsNoBranchTheCorrespondingClaimisNotAddedToTheToken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: null));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РїСѓСЃС‚РѕР№ СЃС‚СЂРѕРєРµ С„РёР»РёР°Р»Р°  branch РЅРµ РґРѕР±Р°РІР»СЏРµС‚СЃСЏ РІ С‚РѕРєРµРЅ.
        public void ChecksThatTheBranchIsNotAddedToTheTokenWhenTheBranchIsEmpty()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: ""));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РµСЃР»Рё С„РёР»РёР°Р» СЃРѕСЃС‚РѕРёС‚ С‚РѕР»СЊРєРѕ РёР· РїСЂРѕР±РµР»РѕРІ, branch РЅРµ РґРѕР±Р°РІР»СЏРµС‚СЃСЏ РІ С‚РѕРєРµРЅ.
        public void CheckingThatIfTheBranchConsistsOnlyOfSpacesTheBranchIsNotAddedToTheToken()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(branch: "   "));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "branch");
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚ РєРѕСЂСЂРµРєС‚РЅРѕРіРѕ РёР·РґР°С‚РµР»СЏ, Р·Р°РґР°РЅРЅРѕРіРѕ РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё.
        public void CheckingThatTheTokenContainsAValidIssuer()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("TestIssuer", jwt.Issuer);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С‚РѕРєРµРЅ СЃРѕРґРµСЂР¶РёС‚ РєРѕСЂСЂРµРєС‚РЅСѓСЋ Р°СѓРґРёС‚РѕСЂРёСЋ, Р·Р°РґР°РЅРЅСѓСЋ РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё.
        public void CheckingThatTheTokenContainsTheCorrectAudience()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("TestAudience", jwt.Audiences.First());
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РІСЂРµРјСЏ РёСЃС‚РµС‡РµРЅРёСЏ С‚РѕРєРµРЅР° Р±РѕР»СЊС€Рµ С‚РµРєСѓС‰РµРіРѕ Рё РЅРµ РїСЂРµРІС‹С€Р°РµС‚ Р·РЅР°С‡РµРЅРёРµ Р·Р°РґР°РЅРЅРѕРµ РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё (60 РјРёРЅСѓС‚).
        public void CheckingThatTheTokensExpirationTimeIsGreaterThanTheCurrentTimeAndDoesNotExceedTheValue()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(61));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ JWT-С‚РѕРєРµРЅ РїРѕРґРїРёСЃР°РЅ Р°Р»РіРѕСЂРёС‚РјРѕРј HMAC SHA256 (HS256).
        public void CheckingThatTheTokenIsSignedWithTheHMACSHA256Algorithm()
        {
            var jwt = TokenHandler.ReadJwtToken(_authService.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("HS256", jwt.Header.Alg);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЂР°Р·РЅС‹Рµ РїРѕР»СЊР·РѕРІР°С‚РµР»Рё РїРѕР»СѓС‡Р°СЋС‚ СЂР°Р·РЅС‹Рµ JWT-С‚РѕРєРµРЅС‹.
        public void ItChecksThatD0ifferentUsersReceiveDifferentTokens()
        {
            var token1 = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 1, email: "a@b.com"));
            var token2 = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 2, email: "c@d.com"));

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕРІС‚РѕСЂРЅР°СЏ РіРµРЅРµСЂР°С†РёСЏ С‚РѕРєРµРЅР° РґР»СЏ РѕРґРЅРѕРіРѕ Рё С‚РѕРіРѕ Р¶Рµ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ СЃРѕР·РґР°С‘С‚ РІР°Р»РёРґРЅС‹Рµ JWT-С‚РѕРєРµРЅС‹
        public void CheckingThatRegeneratingTheTokenCreatesValidTokens()
        {
            var u = CreatingATestUserWithSpecifiedParameters();
            var token1 = _authService.GenerateToken(u);
            var token2 = _authService.GenerateToken(u);

            Assert.True(TokenHandler.CanReadToken(token1));
            Assert.True(TokenHandler.CanReadToken(token2));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РґР°Р¶Рµ РїСЂРё Р±РѕР»СЊС€РѕРј Р·РЅР°С‡РµРЅРёРё РёРґРµРЅС‚РёС„РёРєР°С‚РѕСЂР° (999999)  NameIdentifier СЃРѕС…СЂР°РЅСЏРµС‚СЃСЏ РєРѕСЂСЂРµРєС‚РЅРѕ.
        public void CheckingThatEvenWhenTheNameIdentifierValueIsLargeItIsStoredCorrectly()
        {
            var token = _authService.GenerateToken(CreatingATestUserWithSpecifiedParameters(id: 999999));
            var jwt = TokenHandler.ReadJwtToken(token);

            Assert.Equal("999999", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё Issuer РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ Р·РЅР°С‡РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ "CifraShop".
        public void CheckingThatTheDefaultValueIsUsedWhenTheIssuerIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Issuer", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("CifraShop", jwt.Issuer);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё Audience РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ Р·РЅР°С‡РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ "CifraShop".
        public void CheckingThatTheDefaultValueIsUsedWhenAudienceIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Audience", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.Equal("CifraShop", jwt.Audiences.First());
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё ExpiryMinutes РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ Р·РЅР°С‡РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ 1440 РјРёРЅСѓС‚ (СЃСѓС‚РєРё).
        public void CheckingThatTheDefaultValueIsusedWhenExpiryMinutesIsMissing()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:ExpiryMinutes", null } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(1439));
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(1441));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё РєР»СЋС‡Р° (Jwt:Key) РІ РєРѕРЅС„РёРіСѓСЂР°С†РёРё РІС‹Р±СЂР°СЃС‹РІР°РµС‚СЃСЏ РёСЃРєР»СЋС‡РµРЅРёРµ InvalidOperationException.
        public void CheckingThatAnExceptionIsThrownIfNoKeyIsPresent()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:Key", null } });

            Assert.Throws<InvalidOperationException>(() => svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё СѓСЃС‚Р°РЅРѕРІРєРµ РІСЂРµРјРµРЅРё Р¶РёР·РЅРё С‚РѕРєРµРЅР° 5 РјРёРЅСѓС‚, РѕРЅ РёСЃС‚РµРєР°РµС‚ РІ РїСЂРµРґРµР»Р°С… РѕР¶РёРґР°РµРјРѕРіРѕ  (4вЂ“6 РјРёРЅСѓС‚).
        public void CheckingThatWhenTheTokensLifetimeIsSetItExpiresWithinTheExpectedTime()
        {
            var svc = CreatinganAuthServiceInstanceWithATestJWTConfiguration(new() { { "Jwt:ExpiryMinutes", "5" } });
            var jwt = TokenHandler.ReadJwtToken(svc.GenerateToken(CreatingATestUserWithSpecifiedParameters()));

            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(4));
            Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(6));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С…СЌС€РёСЂРѕРІР°РЅРёРµ РѕРґРЅРѕРіРѕ Рё С‚РѕРіРѕ Р¶Рµ РїР°СЂРѕР»СЏ РІСЃРµРіРґР° РґР°С‘С‚ РѕРґРёРЅР°РєРѕРІС‹Р№ СЂРµР·СѓР»СЊС‚Р°С‚.
        public void ChecksThatPasswordHashingAlwaysProducesTheSameResult()
        {
            var hash1 = _authService.HashPassword("password123");
            var hash2 = _authService.HashPassword("password123");

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЂР°Р·РЅС‹Рµ РїР°СЂРѕР»Рё РґР°СЋС‚ СЂР°Р·РЅС‹Рµ С…СЌС€Рё.
        public void CheckingThatDifferentPasswordsProduceDifferentHashes()
        {
            var hash1 = _authService.HashPassword("password123");
            var hash2 = _authService.HashPassword("password456");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ HashPassword РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅСѓСЋ Base64-СЃС‚СЂРѕРєСѓ РґР»РёРЅРѕР№ 32 Р±Р°Р№С‚Р° (SHA256).
        public void ChecksThatHashPasswordReturnsACorrectString()
        {
            var hash = _authService.HashPassword("test");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            var bytes = Convert.FromBase64String(hash);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С…СЌС€РёСЂРѕРІР°РЅРёРµ РїСѓСЃС‚РѕР№ СЃС‚СЂРѕРєРё РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅС‹Р№ РЅРµРїСѓСЃС‚РѕР№ С…СЌС€.
        public void CheckingThatHashingAnEmptyStringReturnsAValidHash()
        {
            var hash = _authService.HashPassword("");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С…СЌС€РёСЂРѕРІР°РЅРёРµ СЃС‚СЂРѕРєРё СЃ Unicode (РєРёСЂРёР»Р»РёС†Р°) РІРѕР·РІСЂР°С‰Р°РµС‚ РЅРµРїСѓСЃС‚РѕР№ С…СЌС€.
        public void CheckingThatHashingAUnicodeStringReturnsANonEmptyHash()
        {
            var hash = _authService.HashPassword("РїР°СЂРѕР»СЊ123");

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С…СЌС€РёСЂРѕРІР°РЅРёРµ РѕС‡РµРЅСЊ РґР»РёРЅРЅРѕР№ СЃС‚СЂРѕРєРё (10000 СЃРёРјРІРѕР»РѕРІ) РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅС‹Р№ С…СЌС€ РґР»РёРЅРѕР№ 32 Р±Р°Р№С‚Р°.
        public void Checking10kCharHashLength()
        {
            var longPassword = new string('a', 10000);
            var hash = _authService.HashPassword(longPassword);

            Assert.NotNull(hash);
            Assert.Equal(32, Convert.FromBase64String(hash).Length);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ С…СЌС€ РЅРµ СЃРѕРґРµСЂР¶РёС‚ РёСЃС…РѕРґРЅРѕРіРѕ РїР°СЂРѕР»СЏ РІ РѕС‚РєСЂС‹С‚РѕРј РІРёРґРµ.
        public void CheckingHashExcludesPlaintextPassword()
        {
            var password = "secret123";
            var hash = _authService.HashPassword(password);

            Assert.DoesNotContain(password, hash);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РІРѕР·РІСЂР°С‰Р°РµС‚ true РґР»СЏ РєРѕСЂСЂРµРєС‚РЅРѕРіРѕ РїР°СЂРѕР»СЏ.
        public void CheckingCorrectPasswordReturnsTrue()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.True(_authService.VerifyPassword("secret123", hash));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РІРѕР·РІСЂР°С‰Р°РµС‚ false РґР»СЏ РЅРµРІРµСЂРЅРѕРіРѕ РїР°СЂРѕР»СЏ.
        public void CheckingWrongPasswordReturnsFalse()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.False(_authService.VerifyPassword("wrong", hash));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РІРѕР·РІСЂР°С‰Р°РµС‚ false РґР»СЏ РїСѓСЃС‚РѕРіРѕ РїР°СЂРѕР»СЏ.
        public void CheckingEmptyPasswordReturnsFalse()
        {
            var hash = _authService.HashPassword("secret123");

            Assert.False(_authService.VerifyPassword("", hash));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword С‡СѓРІСЃС‚РІРёС‚РµР»СЊРЅР° Рє СЂРµРіРёСЃС‚СЂСѓ: РїР°СЂРѕР»Рё "password" Рё "PASSWORD" РЅРµ РїСЂРѕС…РѕРґСЏС‚ РїСЂРѕРІРµСЂРєСѓ.
        public void CheckingCaseSensitivityRejectsWrongCaseVariants()
        {
            var hash = _authService.HashPassword("Password");

            Assert.False(_authService.VerifyPassword("password", hash));
            Assert.False(_authService.VerifyPassword("PASSWORD", hash));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РІРѕР·РІСЂР°С‰Р°РµС‚ false РїСЂРё РёСЃРїРѕР»СЊР·РѕРІР°РЅРёРё С…СЌС€Р° РѕС‚ РґСЂСѓРіРѕРіРѕ РїР°СЂРѕР»СЏ.
        public void CheckingWrongHashReturnsFalse()
        {
            var otherHash = _authService.HashPassword("other");

            Assert.False(_authService.VerifyPassword("secret123", otherHash));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РІРѕР·РІСЂР°С‰Р°РµС‚ false РїСЂРё РїСѓСЃС‚РѕРј С…СЌС€Рµ.
        public void CheckingEmptyHashReturnsFalse()
        {
            Assert.False(_authService.VerifyPassword("secret123", ""));
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ VerifyPassword РєРѕСЂСЂРµРєС‚РЅРѕ СЂР°Р±РѕС‚Р°РµС‚ СЃ Unicode-РїР°СЂРѕР»СЏРјРё (РєРёСЂРёР»Р»РёС†Р°)
        public void CheckingUnicodePasswordVerification()
        {
            var hash = _authService.HashPassword("РїР°СЂРѕР»СЊ");

            Assert.True(_authService.VerifyPassword("РїР°СЂРѕР»СЊ", hash));
            Assert.False(_authService.VerifyPassword("РїР°СЂРѕР»СЊ2", hash));
        }
    }
}

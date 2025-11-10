using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace VendingMachines.Infrastructure.Services
{
    public static class JwtKeyService
    {
        public static void GenerateJwtAndSetToEnvironment(
            string algorithm, string envVariableName = "JWT_SECRET", string target = "User")
        {
            string secret = GenerateJwtKey(algorithm);

            EnvironmentVariableTarget envTarget = target.ToLower() switch
            {
                "user" => EnvironmentVariableTarget.User,
                "machine" => EnvironmentVariableTarget.Machine,
                _ => throw new ArgumentException("target должен быть 'User' или 'Machine'")
            };

            Environment.SetEnvironmentVariable(envVariableName, secret, envTarget);
        }

        public static string GenerateJwtKey(string algorithm)
        {
            int keyLength = 0;

            switch (algorithm)
            {
                case SecurityAlgorithms.HmacSha256:
                    keyLength = 32;
                    break;

                case SecurityAlgorithms.HmacSha384:
                    keyLength = 48;
                    break;

                case SecurityAlgorithms.HmacSha512:
                    keyLength = 64;
                    break;
            }

            var keyBytes = new byte[keyLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            return Convert.ToBase64String(keyBytes);
        }
    }
}

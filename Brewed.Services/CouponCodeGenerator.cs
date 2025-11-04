using System;
using System.Linq;
using System.Text;

namespace Brewed.Services
{
    public static class CouponCodeGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate(int length = 8, string prefix = null)
        {
            if (length < 4)
                throw new ArgumentException("Coupon code length must be at least 4 characters");

            var codeLength = prefix != null ? length - prefix.Length : length;
            if (codeLength < 4)
                throw new ArgumentException("Coupon code length after prefix must be at least 4 characters");

            var code = new StringBuilder();

            if (!string.IsNullOrEmpty(prefix))
            {
                code.Append(prefix.ToUpper());
            }

            for (int i = 0; i < codeLength; i++)
            {
                code.Append(_chars[_random.Next(_chars.Length)]);
            }

            return code.ToString();
        }

        public static string GenerateFormatted(int segmentLength = 4, int segments = 2)
        {
            if (segmentLength < 3)
                throw new ArgumentException("Segment length must be at least 3 characters");

            if (segments < 1)
                throw new ArgumentException("Number of segments must be at least 1");

            var codeParts = new string[segments];
            for (int i = 0; i < segments; i++)
            {
                var segment = new StringBuilder();
                for (int j = 0; j < segmentLength; j++)
                {
                    segment.Append(_chars[_random.Next(_chars.Length)]);
                }
                codeParts[i] = segment.ToString();
            }

            return string.Join("-", codeParts);
        }
    }
}
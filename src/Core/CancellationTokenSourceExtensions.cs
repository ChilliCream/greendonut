using System.Threading;

namespace GreenDonut
{
    internal static class CancellationTokenSourceExtensions
    {
        public static CancellationToken CreateLinkedCancellationToken(
            this CancellationTokenSource source,
            CancellationToken cancellationToken)
        {
            if (source == null)
            {
                return cancellationToken;
            }

            return CancellationTokenSource.CreateLinkedTokenSource(
                source.Token,
                cancellationToken)
                    .Token;
        }
    }
}

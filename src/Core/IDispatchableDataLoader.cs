using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A <c>DataLoader</c> enhancement for dispatching.
    /// </summary>
    public interface IDispatchableDataLoader
    {
        /// <summary>
        /// Dispatches one or more batch requests.
        /// In case of auto dispatching we just trigger an implicit dispatch
        /// which could mean to interrupt a wait delay. Whereas in a manual
        /// dispatch scenario it could mean to dispatch explicitly.
        /// </summary>
        Task DispatchAsync();
    }
}

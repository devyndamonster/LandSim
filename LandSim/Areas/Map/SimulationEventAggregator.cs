using System.Collections.Concurrent;

namespace LandSim.Areas.Map
{
    public class SimulationEventAggregator
    {
        private readonly Dictionary<Type, List<object>> _handlers = new Dictionary<Type, List<object>>();

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            if (_handlers.ContainsKey(typeof(TEvent)))
            {
                foreach (var handler in _handlers[typeof(TEvent)])
                {
                    ((Action<TEvent>)handler)(eventToPublish);
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (!_handlers.ContainsKey(typeof(TEvent)))
            {
                _handlers[typeof(TEvent)] = new List<object>();
            }
            
            _handlers[typeof(TEvent)].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (_handlers.ContainsKey(typeof(TEvent)))
            {
                _handlers[typeof(TEvent)].Remove(handler);
            }
        }
    }
}

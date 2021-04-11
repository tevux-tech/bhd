using System;
using System.Collections.Generic;
using System.Linq;
using DevBot9.Protocols.Homie;

namespace BlazorHomieDashboard {
    public class HistoryService {
        public static HistoryService Instance { get; } = new HistoryService();

        private HistoryService() {}

        public class FloatHistoryItem {
            public DateTime Date { get; set; }
            public float Value { get; set; }
        }

        private Dictionary<ClientFloatProperty, List<FloatHistoryItem>> _histories = new();

        public void StartHistoryMonitoring(ClientFloatProperty property) {
            _histories[property] = new List<FloatHistoryItem>();

            property.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(ClientFloatProperty.Value)) {
                    _histories[property].Add(new FloatHistoryItem() { Date = DateTime.Now, Value = property.Value });
                }
            };
        }

        public void ClearHistory(ClientFloatProperty property) {
            _histories[property] = new List<FloatHistoryItem>();
        }

        public List<FloatHistoryItem> GetFloatHistory(ClientFloatProperty property) {
            return _histories[property].ToList();
        }
    }
}
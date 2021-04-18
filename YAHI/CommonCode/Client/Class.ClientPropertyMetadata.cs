namespace DevBot9.Protocols.Homie {
    public class ClientPropertyMetadata {
        public string NodeId { get; set; } = "no-id";
        public string PropertyId { get; set; } = "no-id";
        public PropertyType PropertyType { get; set; } = PropertyType.State;
        public string Name { get; set; } = "no-name";
        public DataType DataType { get; set; } = DataType.String;
        public string Format { get; set; } = "";
        public string Unit { get; set; } = "";
        public string InitialValue { get; set; }

        public override string ToString() {
            return PropertyId + ":" + InitialValue;
        }
    }
}

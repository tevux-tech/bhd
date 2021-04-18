namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Possible logical property types. This is NOT defined by Homie convention, but rather and additional constrain added by YAHI. However, it is fully Homie-compliant.
    /// </summary>
    public enum PropertyType {
        State,
        Parameter,
        Command
    }
}

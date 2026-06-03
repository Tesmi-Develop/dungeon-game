namespace Shared.Attributes.Engine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NonSyncedAttribute : Attribute;

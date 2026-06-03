namespace DotTiled.Properties;

/// <summary>
/// Represents an object property.
/// </summary>
public class ObjectProperty : IProperty<uint>
{
  /// <inheritdoc/>
  public required string Name { get; set; }

  /// <inheritdoc/>
  public PropertyType Type => PropertyType.Object;

  public object SourceValue => Value;
  
  /// <summary>
  /// The object identifier referenced by the property.
  /// </summary>
  public required uint Value { get; set; }

  /// <inheritdoc/>
  public IProperty Clone() => new ObjectProperty
  {
    Name = Name,
    Value = Value
  };
}

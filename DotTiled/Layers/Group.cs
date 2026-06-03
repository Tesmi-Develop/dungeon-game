using System.Collections.Generic;

namespace DotTiled.Layers;

/// <summary>
/// Represents a group of layers, to form a hierarchy.
/// </summary>
public class Group : BaseLayer
{
  /// <summary>
  /// The contained sub-layers in the group.
  /// </summary>
  public List<BaseLayer> Layers { get; set; } = [];
}

using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class StoneTile : RuleTile<StoneTile.Neighbor> {
    public class Neighbor : TilingRuleOutput.Neighbor
    {
        public const int NotNullAndNotThis = 3;
        public const int NotNullAndStone = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        return neighbor switch
        {
            Neighbor.NotNullAndNotThis => tile != null && tile != this,
            Neighbor.NotNullAndStone => tile != null && tile != this && tile.GetType() == typeof(StoneTile),
            _ => base.RuleMatch(neighbor, tile),
        };
    }
}
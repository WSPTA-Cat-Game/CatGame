using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class IDTile : RuleTile<IDTile.Neighbor> 
{
    public string id;

    public override bool RuleMatch(int neighbor, TileBase tile) {
        if (tile is RuleOverrideTile ruleOverrideTile)
        {
            tile = ruleOverrideTile.m_InstanceTile;
        }

        IDTile tileAsID = tile as IDTile;

        return neighbor switch
        {
            Neighbor.SameID => tileAsID != null && tileAsID.id == id,
            Neighbor.DiffID => tileAsID != null && tileAsID.id != id,
            _ => base.RuleMatch(neighbor, tile),
        };
    }

    public class Neighbor : TilingRuleOutput.Neighbor
    {
        public const int SameID = 3;
        public const int DiffID = 4;
    }
}
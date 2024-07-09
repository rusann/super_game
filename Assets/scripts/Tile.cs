using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x, y;
    private Item _item;

    public Item temp_item;

    public Item Item {
        get => _item;

        set {
            if (_item == value) return;
            _item = value;
            icon.sprite = _item.sprite;
        }
    }
    public Image icon;
    public Button button;

    public Tile left => x > 0 ? Board.Instance.Tiles[x-1, y] : null;
    public Tile right => x < Board.Instance.Width - 1 ? Board.Instance.Tiles[x+1, y] : null;
    public Tile top => y > 0 ? Board.Instance.Tiles[x, y-1] : null;
    public Tile bottom => y < Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y+1] : null;

    public Tile[] neigs => new [] {left, right, top, bottom};
    private void Start() => button.onClick.AddListener(() => Board.Instance.Select(this));
    
    public bool item_check(Tile tile) {
        if (tile.temp_item == null && temp_item == null) {
            return Item == tile.Item;
        } else if (tile.temp_item != null && temp_item != null) {
            return temp_item == tile.temp_item; 
        } else {
            return temp_item == tile.Item || Item == tile.temp_item;
        }
    }
    public List<Tile> GetConnectedTiles(List<Tile> ex = null) {
        var result = new List<Tile> {this};
        if (ex == null) {
            ex = new List<Tile> {this};
        } else {
            ex.Add(this);
        }

        foreach (Tile t in neigs) {
            if (t != null && !ex.Contains(t) && item_check(t)) result.AddRange(t.GetConnectedTiles(ex));
        }
        return result;
    }
}

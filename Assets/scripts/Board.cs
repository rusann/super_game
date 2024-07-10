using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    private Tile _selectedTile1;
    private Tile _selectedTile2;

    private bool isSwapping = false;
    private bool isMatching = false;
    private bool isShuffling = false;

    private readonly List<Tile> _selection = new List<Tile>();


    private void Awake() => Instance = this;

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;
                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }
        Pop();
    }

    private void Update() {
        if (!CheckMove()) {
            Shuffle();
        }
    }

    public async void Select(Tile tile)
    {
        if (isSwapping || isMatching) return;
        
        if (!_selection.Contains(tile)) _selection.Add(tile);
        if (_selection.Count < 2) return;
        if (!((math.abs(_selection[0].x - _selection[1].x) == 1 && math.abs(_selection[0].y - _selection[1].y) == 0) || (math.abs(_selection[0].x - _selection[1].x) == 0 && math.abs(_selection[0].y - _selection[1].y) == 1))) 
        {
            _selection.Clear();
            return; 
        }
        await Swap(_selection[0], _selection[1]);
        if (CanMatch()) {
            Pop();
        } else {
            await Swap(_selection[0], _selection[1]);
        }
        _selection.Clear();
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++) 
                if (Tiles[x, y].temp_item != null) throw new DataException();
    }

    public async Task Swap(Tile t1, Tile t2)
    {
        isSwapping = true;
        var t1img = t1.icon;
        var t1item = t1.Item;
        var t1Transform = t1.icon.transform;
        var t2Transform = t2.icon.transform;

        var seq = DOTween.Sequence();
        seq.Join(t1Transform.DOMove(t2Transform.position, 0.25f).SetEase(Ease.OutBack)).Join(t2Transform.DOMove(t1Transform.position, 0.25f).SetEase(Ease.OutBack));

        await seq.Play().AsyncWaitForCompletion();

        t1Transform.SetParent(t2.transform);
        t2Transform.SetParent(t1.transform);
        t1.icon = t2.icon;
        t2.icon = t1img;
        t1.Item = t2.Item;
        t2.Item = t1item;
        isSwapping = false;
    }

    private bool CheckMove() {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {   
                var tile = Tiles[x, y];
                if (tile.right) {
                    tile.right.temp_item = tile.Item;
                    tile.temp_item = tile.right.Item;
                    if (CanMatch()) {
                        tile.right.temp_item = null;
                        tile.temp_item = null;
                        Debug.Log($"{x}, {y}");
                        return true;
                    }
                    tile.right.temp_item = null;
                }
                if (tile.bottom) {
                    tile.bottom.temp_item = tile.Item;
                    tile.temp_item = tile.bottom.Item;
                    if (CanMatch()) {
                        tile.bottom.temp_item = null;
                        tile.temp_item = null;
                        Debug.Log($"{x}, {y}");
                        return true;
                    }
                    tile.bottom.temp_item = null;
                }
                tile.temp_item = null;
            }
        }
        return false;
    }

    private bool CanMatch() {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Count() >= 3) {
                    return true;
                }
            }
        }
        return false;
    }

    private async void Pop() {
        isMatching = true;
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                var conTiles = Tiles[x, y].GetConnectedTiles();
                if (conTiles.Count() >= 3) {
                    var animsequence = DOTween.Sequence();

                    foreach (Tile t in conTiles) {
                        animsequence.Join(t.icon.transform.DOScale(Vector3.zero, 0.25f));
                    }
                    await animsequence.Play().AsyncWaitForCompletion();
                    Score.Instance.score += tile.Item.val * conTiles.Count();

                    var appearsequence = DOTween.Sequence();

                    foreach (Tile t in conTiles) {
                        t.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
                        appearsequence.Join(t.icon.transform.DOScale(Vector3.one, 0.25f));
                    }
                    await appearsequence.Play().AsyncWaitForCompletion();
                    x = 0;
                    y = 0;
                    
                }
            }
        }
        isMatching = false;
    }

    private void Shuffle() {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Tiles[x, y].Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    public enum LabIcon { Atom, Flask, Coin, Book, Coat, Trophy, Quest, Flame, Lock, Check }
    public static class LabIcons
    {
        // Hand-authored 16px sprites share an ink outline and shaded reagent palette.
        private static readonly Dictionary<LabIcon, Sprite> icons = new Dictionary<LabIcon, Sprite>();
        public static Sprite Get(LabIcon kind)
        {
            if (icons.TryGetValue(kind, out var sprite) && sprite != null) return sprite;
            var rows = Rows(kind);
            const int size = 16;
            var pixels = new Color32[size * size];
            for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++) pixels[(size - 1 - y) * size + x] = Palette(rows[y][x]);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "LabIcon_" + kind, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 16, 0, SpriteMeshType.FullRect);
            sprite.name = "LabIcon_" + kind;
            icons[kind] = sprite;
            return sprite;
        }
        private static Color32 Palette(char c)
        {
            switch (c)
            {
                case '#': return new Color32(36, 53, 47, 255);
                case 'b': return new Color32(164, 105, 48, 255);
                case 'g': return new Color32(232, 174, 67, 255);
                case 'y': return new Color32(255, 226, 143, 255);
                case 'w': return new Color32(255, 248, 220, 255);
                case 's': return new Color32(136, 173, 162, 255);
                case 't': return new Color32(46, 130, 110, 255);
                case 'm': return new Color32(112, 218, 175, 255);
                case 'r': return new Color32(213, 101, 65, 255);
                default: return new Color32(0, 0, 0, 0);
            }
        }
        private static string[] Rows(LabIcon kind)
        {
            switch (kind)
            {
                case LabIcon.Coin: return new[] {
                    "................", ".....######.....", "...##yyyygg##...", "..#yygggggggb#..",
                    "..#ygg####ggb#..", ".#ygg#yyyy#ggb#.", ".#ygg#ggg#gggb#.", ".#ygg#gggggggb#.",
                    ".#ygg#gggggggb#.", ".#ggg#ggg#gggb#.", ".#ggg#yyyy#ggb#.", "..#ggg####ggb#..",
                    "..#bgggggggbb#..", "...##bbbbbb##...", ".....######.....", "................" };
                case LabIcon.Flame: return new[] {
                    "........#.......", ".......#y#......", "......#yy#......", "......#yg#......",
                    "...#.#yyg#......", "..#y##yygg#.....", "..#yygyygg#.#...", ".#yyggyygg##r#..",
                    ".#ygggyyggggr#..", ".#ggggywygggr#..", ".#ggg#wwygggr#..", "..#gr#wwyggr#...",
                    "..#rr#yyggrr#...", "...#rrgggrr#....", "....#######.....", "................" };
                case LabIcon.Flask: return new[] {
                    ".....######.....", ".....#wssw#.....", "......#ws#......", "......#ws#......",
                    "......#ws#..mm..", ".....#wwss#..m..", "....#ww..ss#....", "....#w....s#....",
                    "...#wwmmmmss#...", "...#wmmwmmts#...", "..#wwmmmmmtts#..", "..#wmmwmmmtts#..",
                    ".#wwmmmmmmttss#.", ".#ssssssssssss#.", "..############..", "................" };
                case LabIcon.Atom: return new[] {
                    "......####......", ".....#mwwm#.....", "..###m#..#m###..", ".#wwm#....#mww#.",
                    ".#smw#....#wms#.", "..#mww#..#wwm#..", "...##ww##ww##...", "...#s##yy##s#...",
                    "...#s##yg##s#...", "...##ww##ww##...", "..#mww#..#wwm#..", ".#smw#....#wms#.",
                    ".#wwm#....#mww#.", "..###m#..#m###..", ".....#mwwm#.....", "......####......" };
                case LabIcon.Book: return new[] {
                    "................", "..############..", ".#rr#wwwwwwwws#.", ".#rg#wwwwwwwss#.",
                    ".#rg#ww####wws#.", ".#rg#wwwwwwwws#.", ".#rg#ww####wws#.", ".#rg#wwwwwwwws#.",
                    ".#rg#ww####wws#.", ".#rg#wwwwwwwws#.", ".#rg#wwwwggwws#.", ".#rg#wwwwgrwws#.",
                    ".#rr#ssssgrsss#.", "..############..", "...#bbbbbbbb#...", "....########...." };
                case LabIcon.Coat: return new[] {
                    ".....##..##.....", "....#ww##ww#....", "..##wws##sww##..", ".#wwwwswsswwww#.",
                    ".#wwwsw##wswww#.", ".#wwsw#ww#wsww#.", ".#ws#ww##wwwsw#.", ".#ss#www#wwwss#.",
                    "..###www#www###.", "....#www#www#...", "....#wsw#wsw#...", "....#wss#wss#...",
                    "....#www#www#...", "....#sss#sss#...", "....#########...", "................" };
                case LabIcon.Trophy: return new[] {
                    "....########....", "..###yyyygg###..", ".#yg#yyyygg#gy#.", ".#g##ywyggg##g#.",
                    ".#g##yyyygg##g#.", "..#g#yyyggg#g#..", "...##yygggb##...", "....#ggggbb#....",
                    ".....#ggbb#.....", "......#gb#......", "......#gb#......", ".....##gb##.....",
                    "....#yyyygb#....", "...#ggggbbbb#...", "...##########...", "................" };
                case LabIcon.Quest: return new[] {
                    ".....######.....", "..####bggb####..", "..#ww#yyyy#ws#..", "..#ww######ws#..",
                    "..#wwwwwwwwws#..", "..#wmw######s#..", "..#wmtwwwwwws#..", "..#wwwwwwwwws#..",
                    "..#wmw######s#..", "..#wmtwwwwwws#..", "..#wwwwwwwwws#..", "..#wss######s#..",
                    "..#wsswwwwwws#..", "..#ssssssssss#..", "..############..", "................" };
                case LabIcon.Lock: return new[] {
                    ".....######.....", "....#ssssss#....", "...#sw####ss#...", "...#ws#..#ss#...",
                    "...#ws#..#ss#...", "..############..", "..#yyyyggggbb#..", "..#yyggggggbb#..",
                    "..#yggg##ggbb#..", "..#ggg#ww#gbb#..", "..#gggg##ggbb#..", "..#gggg##ggbb#..",
                    "..#gggggggbbb#..", "..#bbbbbbbbbb#..", "..############..", "................" };
                case LabIcon.Check: return new[] {
                    "................", "................", "............##..", "...........#wm#.",
                    "..........#wmt#.", ".........#wmt#..", "..##....#wmt#...", ".#wm#..#wmt#....",
                    ".#wmm##wmt#.....", "..#wmmmmt#......", "...#wmmmt#......", "....#mmt#.......",
                    ".....###........", "................", "................", "................" };
                default: return new string[0];
            }
        }
    }
}

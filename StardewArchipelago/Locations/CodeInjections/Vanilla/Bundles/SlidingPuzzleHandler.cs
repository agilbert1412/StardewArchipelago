using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class SlidingPuzzleHandler
    {
        public const int IMAGE_SIZE = 256;
        private const int OFFSET_X = 936 - (IMAGE_SIZE / 2);
        private const int OFFSET_Y = 88;
        private int _size;
        private int NumberOfSquares => _size * _size;
        private Texture2D _backgroundTexture;
        private Rectangle _backgroundTextureSourceRect;
        private Texture2D _todayTexture;
        private int[] _squarePositions;
        private Dictionary<int, List<int>> _validMovesCache;

        public SlidingPuzzleHandler(IModHelper modHelper, Texture2D backgroundTexture, int blockSize)
        {
            _size = blockSize;
            //_size = new Random().Next(2, 7);
            _backgroundTexture = backgroundTexture;
            _backgroundTextureSourceRect = new Rectangle(0, 40, 120, 82);
            var allBundleTextures = BundleIcons.GetAllBundleIcons(modHelper);
            var random = Utility.CreateDaySaveRandom();
            var chosenTexture = allBundleTextures[random.Next(allBundleTextures.Count)];
            _todayTexture = chosenTexture;
            _squarePositions = new int[NumberOfSquares];
            _validMovesCache = new Dictionary<int, List<int>>();
            for (var i = 0; i < NumberOfSquares; i++)
            {
                _squarePositions[i] = i;
            }

            ShufflePuzzle(random);
        }


        private void ShufflePuzzle(Random random)
        {
            int NumberOfSwaps = NumberOfSquares * NumberOfSquares;
            for (int i = 0; i < NumberOfSquares; i++)
            {
                if (_validMovesCache.ContainsKey(i))
                {
                    continue;
                }

                _validMovesCache.Add(i, CalculateValidMoves(i));
            }
            var holePosition = _squarePositions.Last();
            for (int i = 0; i < NumberOfSwaps; i++)
            {
                var validMoves = _validMovesCache[holePosition];
                var indexMove = random.Next(validMoves.Count);
                var swapMove = validMoves[indexMove];
                MoveSquare(swapMove);
                holePosition = swapMove;
            }
        }

        public void DrawPuzzle(SpriteBatch spriteBatch, int xPositionOnScreen, int yPositionOnScreen)
        {
            var backgroundScale = 4f;
            var backgroundX = xPositionOnScreen + OFFSET_X + (IMAGE_SIZE / 2) - (_backgroundTextureSourceRect.Width / 2 * backgroundScale);
            var backgroundY = yPositionOnScreen + 56;
            var backgroundPosition = new Vector2(backgroundX, backgroundY);
            spriteBatch.Draw(_backgroundTexture, backgroundPosition, _backgroundTextureSourceRect, Color.White, 0.0f, Vector2.Zero, backgroundScale, SpriteEffects.None, 0.15f);
            for (var i = 0; i < NumberOfSquares; i++)
            {
                DrawPuzzleSquare(spriteBatch, i, xPositionOnScreen + OFFSET_X, yPositionOnScreen + OFFSET_Y);
            }
        }

        private void DrawPuzzleSquare(SpriteBatch spriteBatch, int squareIndex, int originX, int originY)
        {
            var squareX = squareIndex % _size;
            var squareY = squareIndex / _size;
            var x = originX + (ScaledSquareWidth * squareX);
            var y = originY + (ScaledSquareWidth * squareY);
            var position = new Vector2(x, y);
            if (_squarePositions[squareIndex] == NumberOfSquares - 1)
            {
                //var xSourceRect = new Rectangle(268, 471, 16, 16);
                //var xScale = (SquareWidth / 32f);
                //var quarterSquare = ScaledSquareWidth / 4;
                //var xPosition = new Vector2(position.X + quarterSquare, position.Y + quarterSquare);
                //spriteBatch.Draw(Game1.mouseCursors, xPosition, xSourceRect, new Color(255, 255, 255, 191), 0.0f,
                //    Vector2.Zero, xScale, SpriteEffects.None, 0.15f);
                return;
            }

            var currentTextureX = _squarePositions[squareIndex] % _size;
            var currentTextureY = _squarePositions[squareIndex] / _size;
            
            var sourceRectX = SquareWidth * currentTextureX;
            var sourceRectY = SquareWidth * currentTextureY;

            var sourceRect = new Rectangle(sourceRectX, sourceRectY, SquareWidth, SquareWidth);
            spriteBatch.Draw(_todayTexture, position, sourceRect, Color.White, 0.0f,
                Vector2.Zero, SquareScale, SpriteEffects.None, 0.15f);
        }

        public bool ReceiveLeftClick(int x, int y)
        {
            x = x - OFFSET_X;
            y = y - OFFSET_Y;

            var squareX = x / ScaledSquareWidth;
            var squareY = y / ScaledSquareWidth;
            var squareIndex = squareY * _size + squareX;
            if (!CanMoveSquare(squareIndex))
            {
                return false;
            }
            MoveSquare(squareIndex);
            return true;

        }

        public bool IsPuzzleSolved()
        {
            for (int i = 0; i < NumberOfSquares; i++)
            {
                if (_squarePositions[i] != i)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanMoveSquare(int square)
        {
            return CanMoveSquare(square, out _);
        }

        public bool CanMoveSquare(int square, out int holePosition)
        {
            holePosition = GetHolePosition();
            if (square < 0 || square >= NumberOfSquares || holePosition < 0 || holePosition >= NumberOfSquares || square == holePosition)
            {
                return false;
            }

            if (!_validMovesCache.ContainsKey(holePosition))
            {
                _validMovesCache.Add(holePosition, CalculateValidMoves(holePosition));
            }

            return _validMovesCache[holePosition].Contains(square);
        }
        private List<int> CalculateValidMoves(int holePosition)
        {
            var holeX = holePosition % _size;
            var holeY = holePosition / _size;
            var validMoves = new List<int>();
            if (holeX > 0)
            {
                validMoves.Add(holePosition-1);
            }
            if (holeX < _size-1)
            {
                validMoves.Add(holePosition + 1);
            }
            if (holeY > 0)
            {
                validMoves.Add(holePosition - _size);
            }
            if (holeY < _size - 1)
            {
                validMoves.Add(holePosition + _size);
            }

            return validMoves;
        }

        public void MoveSquare(int square)
        {
            if (!CanMoveSquare(square, out var holePosition))
            {
                return;
            }

            Swap(square, holePosition);
        }

        private void Swap(int position1, int position2)
        {
            (_squarePositions[position1], _squarePositions[position2]) = (_squarePositions[position2], _squarePositions[position1]);
        }

        public int GetHolePosition()
        {
            for (var i = 0; i < NumberOfSquares; i++)
            {
                if (_squarePositions[i] == NumberOfSquares - 1)
                {
                    return i;
                }
            }

            throw new ArgumentException("No hole in the current puzzle. Something went very wrong");
        }

        private int SquareWidth => _todayTexture.ActualWidth / _size;
        private float SquareScale => (float)IMAGE_SIZE / (float)_todayTexture.ActualWidth;
        private int ScaledSquareWidth => (int)Math.Round(SquareWidth * SquareScale);
    }
}

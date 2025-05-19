using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.HomeRenovations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class SlidingPuzzleHandler
    {
        public const int IMAGE_SIZE = 256;
        private const int OFFSET_X = 936 - (IMAGE_SIZE / 2);
        private const int OFFSET_Y = 88;
        private const int SIZE = 4;
        private const int NUMBER_OF_SQUARES = SIZE * SIZE;
        private Texture2D _backgroundTexture;
        private Rectangle _backgroundTextureSourceRect;
        private Texture2D _todayTexture;
        private int[] _squarePositions;

        public SlidingPuzzleHandler(IModHelper modHelper, Texture2D backgroundTexture)
        {
            _backgroundTexture = backgroundTexture;
            _backgroundTextureSourceRect = new Rectangle(0, 40, 120, 82);
            var allBundleTextures = BundleIcons.GetAllBundleIcons(modHelper);
            var random = Utility.CreateDaySaveRandom();
            var chosenTexture = allBundleTextures[random.Next(allBundleTextures.Count)];
            _todayTexture = chosenTexture;
            _squarePositions = new int[NUMBER_OF_SQUARES];
            for (var i = 0; i < NUMBER_OF_SQUARES; i++)
            {
                _squarePositions[i] = i;
            }

            ShufflePuzzle(random);
        }


        private void ShufflePuzzle(Random random)
        {
            const int NumberOfSwaps = 1000;
            for (int i = 0; i < NumberOfSwaps; i++)
            {
                var indexMove = random.Next(NUMBER_OF_SQUARES);
                MoveSquare(indexMove);
            }
        }

        public void DrawPuzzle(SpriteBatch spriteBatch, int xPositionOnScreen, int yPositionOnScreen)
        {
            var backgroundScale = 4f;
            var backgroundX = xPositionOnScreen + OFFSET_X + (IMAGE_SIZE / 2) - (_backgroundTextureSourceRect.Width / 2 * backgroundScale);
            var backgroundY = yPositionOnScreen + 56;
            var backgroundPosition = new Vector2(backgroundX, backgroundY);
            spriteBatch.Draw(_backgroundTexture, backgroundPosition, _backgroundTextureSourceRect, Color.White, 0.0f, Vector2.Zero, backgroundScale, SpriteEffects.None, 0.15f);
            for (var i = 0; i < NUMBER_OF_SQUARES; i++)
            {
                DrawPuzzleSquare(spriteBatch, i, xPositionOnScreen + OFFSET_X, yPositionOnScreen + OFFSET_Y);
            }
        }

        private void DrawPuzzleSquare(SpriteBatch spriteBatch, int squareIndex, int originX, int originY)
        {
            if (_squarePositions[squareIndex] == NUMBER_OF_SQUARES - 1)
            {
                return;
            }

            var squareX = squareIndex % SIZE;
            var squareY = squareIndex / SIZE;
            var currentTextureX = _squarePositions[squareIndex] % SIZE;
            var currentTextureY = _squarePositions[squareIndex] / SIZE;
            

            var x = originX + (ScaledSquareWidth * squareX);
            var y = originY + (ScaledSquareWidth * squareY);

            var sourceRectX = SquareWidth * currentTextureX;
            var sourceRectY = SquareWidth * currentTextureY;

            var position = new Vector2(x, y);
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
            var squareIndex = squareY * SIZE + squareX;
            if (!CanMoveSquare(squareIndex))
            {
                return false;
            }
            MoveSquare(squareIndex);
            return true;

        }

        public bool IsPuzzleSolved()
        {
            for (int i = 0; i < NUMBER_OF_SQUARES; i++)
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
            if (square < 0 || square >= NUMBER_OF_SQUARES || holePosition < 0 || holePosition >= NUMBER_OF_SQUARES || square == holePosition)
            {
                return false;
            }

            var squareX = square % SIZE;
            var squareY = square / SIZE;
            var holeX = holePosition % SIZE;
            var holeY = holePosition / SIZE;
            if (squareX == holeX)
            {
                return Math.Abs(squareY - holeY) == 1;
            }
            if (squareY == holeY)
            {
                return Math.Abs(squareX - holeX) == 1;
            }

            return false;
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
            for (var i = 0; i < NUMBER_OF_SQUARES; i++)
            {
                if (_squarePositions[i] == NUMBER_OF_SQUARES - 1)
                {
                    return i;
                }
            }

            throw new ArgumentException("No hole in the current puzzle. Something went very wrong");
        }

        private int SquareWidth => _todayTexture.ActualWidth / SIZE;
        private float SquareScale => (float)IMAGE_SIZE / (float)_todayTexture.ActualWidth;
        private int ScaledSquareWidth => (int)Math.Round(SquareWidth * SquareScale);
    }
}

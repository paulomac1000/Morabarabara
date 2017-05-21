﻿using SFML;
using SFML.Graphics;
using SFML.System;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Morabara.Models
{
    public class Field
    {
        public Field(int id, int positionX, int positionY)
        {
            Id = id;
            PositionX = positionX;
            PositionY = positionY;

            BelongsToThree = false;
            TakenBy = TakenBy.Nobody;
        }

        public int Id { get; }

        private TakenBy takenBy;
        public TakenBy TakenBy
        {
            get
            {
                return takenBy;
            }
            set
            {
                switch (value)
                {
                    case TakenBy.Nobody:
                        Circle = new CircleShape(Setting.BallRadius)
                        {
                            Position = new Vector2f(PositionX, PositionY),
                            FillColor = new Color(Color.Transparent)
                        };
                        break;

                    case TakenBy.Player:
                        var playerBallTexture = new Texture("Data/Texture/czerwona kulka.png");
                        if (playerBallTexture.CPointer == IntPtr.Zero)
                        {
                            MessageBox.Show("Could't load Data/Texture/czerwona kulka.png");
                            throw new LoadingFailedException("texture");
                        }
                        Circle = new CircleShape(Setting.BallRadius)
                        {
                            Position = new Vector2f(PositionX, PositionY),
                            Texture = playerBallTexture
                        };
                        break;

                    case TakenBy.Computer:
                        var computerBallTexture = new Texture("Data/Texture/niebieska kulka.png");
                        if (computerBallTexture.CPointer == IntPtr.Zero)
                        {
                            MessageBox.Show("Could't load Data/Texture/niebieska kulka.png");
                            throw new LoadingFailedException("texture");
                        }
                        Circle = new CircleShape(Setting.BallRadius)
                        {
                            Position = new Vector2f(PositionX, PositionY),
                            Texture = computerBallTexture
                        };
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                takenBy = value;
            }
        }

        public bool BelongsToThree { get; set; }

        public int PositionX { get; }
        public int PositionY { get; }

        public CircleShape Circle { get; private set; }
    }
}

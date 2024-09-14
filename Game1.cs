using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TDShooter.Ships;
using TDShooter.Collisions;
using System;
using Microsoft.Xna.Framework.Audio;

namespace TDShooter
{
    public class Game1 : Game
    {
        // GraphicsDeviceManager for the game
        private GraphicsDeviceManager _graphics;

        // SpriteBatch for drawing
        private SpriteBatch _spriteBatch;

        // Spritefont for rendering text
        private SpriteFont _text;

        // The background texture for the game
        private Texture2D _background;

        /// <summary>
        /// tracks if the player has been dead or not
        /// </summary>
        private bool _isPlayerDead = false;

        private bool _gameOver;

        // The player's sprite
        private PlayerShip _player;

        // The list of enemies
        private EnemyShip[] _enemies = new EnemyShip[5];

        private Random rand = new Random();

        private SoundEffect _loseSFX;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Mouse.SetCursor(MouseCursor.Crosshair);
            

            _graphics.PreferredBackBufferHeight = 900;
            _graphics.PreferredBackBufferWidth = 1600;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            _player = new PlayerShip();

            double minMass = 1.0 - (1.0 * 0.15);
            double maxMass = 1.0 + (1.0 * 0.15);

            for (int i = 0; i < _enemies.Length; i++)
            {
                double randomMass = rand.NextDouble() * (maxMass - minMass) + minMass;
                _enemies[i] = new EnemyShip() { Position = new Vector2(i * 300, 100), Mass = (float)randomMass };
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("darkPurple");
            _player.LoadContent(Content);
            _loseSFX = Content.Load<SoundEffect>("sfx_lose");
            _text = Content.Load<SpriteFont>("Baskervville");

            foreach(var enemy in _enemies)
            {
                enemy.LoadContent(Content);
            }

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
           

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                ResetGame();
            }

            if (_gameOver)
            {
                return;
            }

            if (CheckAllEnemiesDead())
            {
                RespawnEnemies();
            }

            _player.Update(gameTime);
            _player.CheckShotsForCollision(_enemies);

            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime, _player.Position);
                enemy.CheckShotsForCollision(_player);

                if (enemy.Hitbox.CollidesWith(_player.Hitbox))
                {
                    enemy.IsColliding = true;

                    Vector2 collisionAxis = enemy.Position - _player.Position;
                    collisionAxis.Normalize();
                    float angle = (float)System.Math.Acos(Vector2.Dot(collisionAxis, Vector2.UnitX));

                    float m0 = enemy.Mass;
                    float m1 = _player.Mass;

                    Vector2 u0 = Vector2.Transform(enemy.Velocity, Matrix.CreateRotationZ(-angle));

                    Vector2 v0;

                    v0.X = ((m0 - m1) / (m0 + m1)) * u0.X + (2 * m1 / (m0 + m1));
                    v0.Y = u0.Y;

                    enemy.Velocity = Vector2.Transform(v0, Matrix.CreateRotationZ(angle));
                    enemy.Position += enemy.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
               
            }

            for (int i = 0; i < _enemies.Length; i++)
            {
                for (int j = i + 1; j < _enemies.Length; j++)
                {
                    if (_enemies[i].Hitbox.CollidesWith(_enemies[j].Hitbox) || _enemies[j].Hitbox.CollidesWith(_player.Hitbox))
                    {
                        _enemies[i].IsColliding = true;
                        _enemies[j].IsColliding = true;

                        Vector2 collisionAxis = _enemies[i].Position - _enemies[j].Position;
                        collisionAxis.Normalize();
                        float angle = (float)System.Math.Acos(Vector2.Dot(collisionAxis, Vector2.UnitX));

                        float m0 = _enemies[i].Mass;
                        float m1 = _enemies[j].Mass;

                        Vector2 u0 = Vector2.Transform(_enemies[i].Velocity, Matrix.CreateRotationZ(-angle));
                        Vector2 u1 = Vector2.Transform(_enemies[j].Velocity, Matrix.CreateRotationZ(-angle));

                        Vector2 v0;
                        Vector2 v1;

                        v0.X = ((m0 - m1) / (m0 + m1)) * u0.X + (2 * m1 / (m0 + m1)) * u1.X;
                        v1.X = (2 * m0 / (m0 + m1)) * u0.X + ((m1 - m0) / (m0 + m1)) * u1.X;
                        v0.Y = u0.Y;
                        v1.Y = u1.Y;

                        _enemies[i].Velocity = Vector2.Transform(v0, Matrix.CreateRotationZ(angle));
                        _enemies[j].Velocity = Vector2.Transform(v1, Matrix.CreateRotationZ(angle));

                        _enemies[i].Position += _enemies[i].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        _enemies[j].Position += _enemies[j].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                }
            }

            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the different game elements to the screen
        /// </summary>
        /// <param name="gameTime">The current time of the game</param>
        protected override void Draw(GameTime gameTime)
        {
            

            _spriteBatch.Begin();

            if (_player.Dead && !_isPlayerDead)
            {
                _isPlayerDead = true;
                _loseSFX.Play();
                RemoveEnemies();
                _gameOver = true;
            }


            if (_player.Dead)
            {
                // Fade the screen to black
                _spriteBatch.Draw(_background, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.Black * 0.1f);    
                _spriteBatch.DrawString(_text, "Game Over!", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight / 2), Color.White);
                _spriteBatch.DrawString(_text, "Press ESC to exit, or R to play again", _text.MeasureString("Press ESC to exit, or R to play again") / 2, Color.White);
                
                if(_player.Score < 5)
                {
                    _spriteBatch.DrawString(_text, "Having troubles? Aim with the mouse and fire with left click!", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 400, _graphics.PreferredBackBufferHeight / 2 + 50), Color.White);
                }
                else
                {
                    _spriteBatch.DrawString(_text, "You took out " + _player.Score + " enemies!", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 175, _graphics.PreferredBackBufferHeight / 2 + 50), Color.White);
                }
            }
            else
            {
                // Calculate the number of times the background texture needs to be repeated horizontally and vertically
                int horizontalRepeats = (int)Math.Ceiling((float)_graphics.PreferredBackBufferWidth / _background.Width);
                int verticalRepeats = (int)Math.Ceiling((float)_graphics.PreferredBackBufferHeight / _background.Height);

                // Fill the background with the texture 
                for (int i = 0; i < horizontalRepeats; i++)
                {
                    for (int j = 0; j < verticalRepeats; j++)
                    {
                        _spriteBatch.Draw(_background, new Vector2(i * _background.Width, j * _background.Height), Color.White);
                    }
                }

                _player.Draw(gameTime, _spriteBatch);
                foreach (var enemy in _enemies)
                {
                    if (!enemy.Dead)
                    {
                        enemy.Draw(gameTime, _spriteBatch);
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Removes all enemies at the end of the game
        /// </summary>
        public void RemoveEnemies()
        {
            for(int i = 0; i < _enemies.Length; i++)
            {
                _enemies[i] = null;
            }
        }

        /// <summary>
        /// Checks if all enemies are dead
        /// </summary>
        /// <returns>True if they're all dead, false otherwise</returns>
        public bool CheckAllEnemiesDead()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (!_enemies[i].Dead)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Respawns the enemies after they all die
        /// </summary>
        public void RespawnEnemies()
        {
            RemoveEnemies();

            double minMass = 1.0 - (1.0 * 0.15);
            double maxMass = 1.0 + (1.0 * 0.15);

            for (int i = 0; i < _enemies.Length; i++)
            {
                double randomMass = rand.NextDouble() * (maxMass - minMass) + minMass;
                _enemies[i] = new EnemyShip() { Position = new Vector2(i * 300, 100), Mass = (float)randomMass };
                _enemies[i].LoadContent(Content);
            }
        }

        /// <summary>
        /// Resets the game to its starting state
        /// </summary>
        public void ResetGame()
        {
            _player = new PlayerShip();
            _player.LoadContent(Content);
            _isPlayerDead = false;
            _gameOver = false;
            RespawnEnemies();
        }
    }
}

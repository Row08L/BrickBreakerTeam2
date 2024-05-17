﻿/*  Created by: Team 2!
 *  Project: Brick Breaker
 *  Date: 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown;

        // Game values
        int lives;
        int levelNumber = 3;
        Score score;
        List<MiniScores> comboAdds = new List<MiniScores>();
        int scoreAngle = 0;
        int scoreDirection = 1;
        int scoreSize = 50;

        // Paddle and Ball objects
        Paddle paddle = new Paddle(0, 0, 0, 0, 0, Color.White);
        Ball ball;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush paddleBrush = new SolidBrush(Color.White);
        SolidBrush ballBrush = new SolidBrush(Color.White);

        GraphicsPath paddleCircle = new GraphicsPath();
        GraphicsPath ballCircle = new GraphicsPath();
        Region ballRegion = new Region();
        Region leftPaddleRegion = new Region();
        Region rightPaddleRegion = new Region();

        Region mirroredLeftPaddleRegion = new Region();
        Region mirroredRightPaddleRegion = new Region();
        GraphicsPath mirroredPaddleCircle = new GraphicsPath();

        Region[] checkRegions = new Region[] { null, null, null, null };

        bool restartLevel = false;

        //cursor Pos

        public static int lastCursorX;


        // slow mode (testing)

        bool slow;

        //mouse move
        bool mouseMoving = false;

        //debuff list

        public static List<Debuff> debuffs = new List<Debuff>();

        // debuff collected

        public static bool debuffCollected = false;

        public static Debuff SDC;

        // debuff? which one

        public static bool dB1, dB2, dB3, dB4, dB5 = false;

        float mirroredBallX;

        int mirroredPaddleX;

        int duration1, duration2, duration3, duration4, duration5;
        int vineLocatoin = 130;
        int grow;

        //bouncing off side of paddle
        float slope;
        bool leftCircleCollision = false;
        bool rightCircleCollision = false;

        //RANOM
        Random rand = new Random();

        //whiteboy
        PictureBox whiteBoy = new PictureBox();

        List<PictureBox> debuff1 = new List<PictureBox>();

        // list of balls

        List<Ball> freakyBalls = new List<Ball>();

        int drawBall;

        bool drawTheBall;

        Color debuffColor;

        //Evil Skull Man

        PictureBox evilSkullMan = new PictureBox();



    
        #endregion

        public GameScreen()
        {
            InitializeComponent();
            blocks = Block.LevelChanger(levelNumber, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            OnStart();
        }


        public void OnStart()
        {
            Cursor.Hide();
            //set life counter
            lives = 4;
            score = new Score(0, 1);

            //set all button presses to false.
            leftArrowDown = rightArrowDown = false;

            // setup starting paddle values and create paddle object
            paddle = new Paddle((this.Width / 2) - (paddle.width / 2), this.Height - paddle.height - 60, 80, 20, 25, Color.White);

            updateCurve();

            // setup starting ball values
            int ballX = this.Width / 2 - 10;
            int ballY = this.Height - paddle.height - 80;

            // Creates a new ball
            int speedMod = 2;
            float xSpeed = 15 * speedMod;
            float ySpeed = -3 * speedMod;
            int ballSize = 20;
            ball = new Ball(ballX, ballY, Convert.ToInt16(xSpeed), Convert.ToInt16(ySpeed), ballSize);
            updateBallStorage();

            // start the game engine loop


            if (debuffs.Count != 0)
            {
                debuffs.Clear();
            }
            dB1 = false;
            dB2 = false;
            dB3 = false;
            dB4 = false;
            dB5 = false;


            gameTimer.Enabled = true;

        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    break;
                case Keys.K:
                    slow = true;
                    break;
                case Keys.Space:
                    if (!restartLevel)
                    {
                        restartLevel = true;
                    }
                    break;
                //testing
                case Keys.P:
                    gameTimer.Enabled = false;
                    break;
                default:
                    break;
            }

        }


        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.K:
                    slow = false;
                    break;
                //testing
                case Keys.P:
                    gameTimer.Enabled = true;
                    break;
                default:
                    break;
            }
        }

        //  RectangleF prevPosition;

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (blocks.Count() == 0)
            {
                levelNumber++;
                blocks = Block.LevelChanger(levelNumber, this.Size);
            }
            Point mouse = this.PointToClient(Cursor.Position);

            int brickTime = 0;
            // Arrow key movements
            if (leftArrowDown && paddle.x > 20)
            {
                paddle.Move("left");
                updateCurve();
                mouseMoving = false;
            }
            if (rightArrowDown && paddle.x < (this.Width - paddle.width - 20))
            {
                paddle.Move("right");
                updateCurve();
                mouseMoving = false;
            }

            //mouse movement
            if (!mouseMoving)
            {
                Cursor.Position = this.PointToScreen(new Point(paddle.x + (paddle.width / 2), paddle.y + (paddle.height / 2)));
            }
            else
            {
                paddle.x = mouse.X - (paddle.width / 2);
                updateCurve();

                if (mouse.X < paddle.width / 2 + 20)
                {
                    Cursor.Position = this.PointToScreen(new Point(0 + paddle.width / 2 + 20, paddle.y + paddle.height / 2));
                }

                if (mouse.X > this.Width - paddle.width / 2 - 20)
                {
                    Cursor.Position = this.PointToScreen(new Point(this.Width - paddle.width / 2 - 20, paddle.y + paddle.height / 2));
                }
            }

            if (!restartLevel)
            {
                ball.x = paddle.x + (paddle.width / 2) - (ball.size / 2);
                ball.y = paddle.y - 25;
            }
            else //game running loop
            {
                brickTime = 0;

                // Move ball
                foreach (Ball b in freakyBalls)
                {
                    b.Move();
                }

                ball.Move();


                // Check for collision with top and side walls
                foreach (Ball b in freakyBalls)
                {
                    b.WallCollision(this);
                }

                ball.WallCollision(this);

                // Check for ball hitting bottom of screen

                if (ball.BottomCollision(this))
                {
                    // SoundPlayer lifesubtracted = new SoundPlayer(Properties.Resources.lifesubtracted);
                    score.RemoveCombo();
                    scoreSize = 50;

                    ball.ySpeed *= -1;
                    lives--;
                    restartLevel = false;
                    //lifesubtracted.Play();

                    freakyBalls.Clear();

                    // Moves the ball back to origin
                    ball.x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                    ball.y = (this.Height - paddle.height) - 85;

                    if (lives == 0)
                    {
                        gameTimer.Enabled = false;
                        OnEnd();
                    }


                }

                for (int i = freakyBalls.Count; i > 0; i--)
                {
                    if (freakyBalls[i - 1].y > this.Height)
                    {
                        freakyBalls.RemoveAt(i - 1);
                        lives--;

                        if (lives == 0)
                        {
                            gameTimer.Enabled = false;
                            OnEnd();
                        }
                    }
                }



                updateBallStorage();
                slope = derivitive();


                //slope momentum bounces
                if (leftCircleCollision)
                {
                    float momentumPercent = 1 - (slope / 100) - (paddle.speed / 2);
                    float yMultiplier = 1 - momentumPercent;
                    ball.ySpeed -= -1 * yMultiplier;
                    ball.xSpeed -= -1 * momentumPercent;
                }
                if (rightCircleCollision)
                {
                    float momentumPercent = 1 - (slope / 100) - (paddle.speed / 2);
                    float yMultiplier = 1 - momentumPercent;
                    ball.ySpeed += -1 * yMultiplier;
                    ball.xSpeed += -1 * momentumPercent;
                }
                //attempt at using angle between vectors to calculate new vector
                /*
                //calculates angle between ball vector and derivative using dot product of vectors
                if (leftCircleCollision || rightCircleCollision)
                {
                    PointF newDirection = new PointF();
                    double taco = Math.Acos((ball.xSpeed + (ball.ySpeed * slope)) / (Math.Sqrt(Math.Pow(ball.xSpeed, 2) + Math.Pow(ball.ySpeed, 2)) * Math.Sqrt(Math.Pow(slope, 2) + 1)));
                    double theta = Math.Atan2(ball.ySpeed - slope, ball.xSpeed - 1);

                    if (theta > 90)
                    {
                        theta = 180 - theta;
                    }
                    theta *= Math.PI / 180;
                    float colX = ball.x + ball.size - paddle.x;
                    float colY = (float)Math.Sqrt(400 - Math.Pow(colX, 2));

                    if (leftCircleCollision) //left collision
                    {
                        if (ball.xSpeed > 0 && ball.ySpeed > 0)
                        {
                            if (theta < Math.PI / 2)
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 2 * theta); //rotate cc
                            else
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 360 - (2 * theta)); //rotate ccw
                        }
                        else if (ball.xSpeed < 0 && ball.ySpeed > 0)
                        {
                            if (theta < Math.PI / 2)
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 360 - (2 * theta)); //rotate cc
                            else
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 2 * theta); //rotate ccw
                        }
                    }
                    else //rotate clocklwise
                    {
                        if (ball.xSpeed < 0 && ball.ySpeed > 0)
                        {
                            if (theta < Math.PI / 2)
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 2 * theta); //rotate cc
                            else
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 360 - (2 * theta)); //rotate ccw
                        }
                        else if (ball.xSpeed > 0 && ball.ySpeed > 0)
                        {
                            if (theta < Math.PI / 2)
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 360 - (2 * theta)); //rotate cc
                            else
                                newDirection = Block.RotatePoint(prevPosition, new PointF(colX, colY), 2 * theta); //rotate ccw
                        }
                    }
                    ball.xSpeed = -(newDirection.X - colX);
                    ball.ySpeed = -(newDirection.Y - colY);
                }
                */

                //speed capping code
                const float MAXSPEED = 20;
                const float MINSPEED = 10;

                if (Math.Abs(ball.xSpeed) < MINSPEED && Math.Abs(ball.ySpeed) < MINSPEED) //makes really slow balls less slow
                {
                    while (Math.Abs(ball.xSpeed) < MINSPEED || Math.Abs(ball.ySpeed) < MINSPEED)
                    {
                        ball.xSpeed *= (float)1.25;
                        ball.ySpeed *= (float)1.25;
                    }
                }

                while (Math.Abs(ball.xSpeed) > MAXSPEED || Math.Abs(ball.ySpeed) > MAXSPEED) //makes really fast balls less fast
                {
                    if (Math.Abs(ball.xSpeed) > MAXSPEED)
                    {
                        float diff = MAXSPEED / ball.xSpeed;
                        ball.xSpeed *= Math.Abs(diff);
                        ball.ySpeed *= Math.Abs(diff);
                    }
                    if (Math.Abs(ball.ySpeed) > MAXSPEED)
                    {
                        float diff = MAXSPEED / ball.ySpeed;
                        ball.xSpeed *= Math.Abs(diff);
                        ball.ySpeed *= Math.Abs(diff);
                    }
                }

                if (Math.Abs(ball.ySpeed) < MINSPEED)
                {
                    float diff = MINSPEED / ball.ySpeed;
                    ball.ySpeed *= Math.Abs(diff);
                }


                // Check for collision of ball with paddle, (incl. paddle movement)
                foreach (Ball b in freakyBalls)
                {
                    b.PaddleCollision(paddle);
                }

                ball.PaddleCollision(paddle);

                // SoundPlayer brickbroken = new SoundPlayer(Properties.Resources.brickbroken);


                // Check if ball has collided with any blocks
                foreach (Block b in blocks)
                {
                    if (brickTime == 0)
                    {
                        if (ball.BlockCollision(b))
                        {
                            
                            comboAdds.Add(new MiniScores(100 * score.comboCounter + "", new Point((int)paddle.x + rand.Next(-50, 50), (int)paddle.y - 50 + rand.Next(-50, 50)), 255));
                            score.AddToScore(100);
                            scoreSize += 1;


                            b.hp--;
                            if (b.hp == 0)
                            {
                                blocks.Remove(b);
                                int chance = 40;

                                if (rand.Next(1, 100) <= chance)
                                {
                                    int check = rand.Next(1, 100);
                                    int o = 0;

                                    if (check > 10 && check < 20)
                                    {
                                        o = 1;
                                        debuffColor = Color.Green;
                                    }
                                    else if (check > 20 && check < 50)
                                    {
                                        o = 2;
                                        debuffColor = Color.Pink;
                                    }
                                    else if (check == 50)
                                    {
                                        o = 3;
                                        debuffColor = Color.Black;
                                    }
                                    else if (check > 51 && check < 62)
                                    {
                                        o = 4;
                                        debuffColor = Color.White;
                                    }
                                    else
                                    {
                                        o = 5;
                                        debuffColor = Color.Silver;
                                    }

                                    Debuff newDebuff = new Debuff(o, b.hitBox.X + b.hitBox.Width / 2, b.hitBox.Y + b.hitBox.Width, debuffColor);

                                    debuffs.Add(newDebuff);
                                }
                            }
                            else
                            {
                                b.currentTexture++;
                                b.texture = b.textures[b.currentTexture];
                            }
                            brickTime = 20;

                            if (blocks.Count == 0)
                            {
                                gameTimer.Enabled = false;
                                OnEnd();
                            }
                            break;
                        }
                    }
                }
            }


            #region Debuff Area
            foreach (Debuff d in debuffs)
            {
                d.PaddleCollision(paddle, d);
            }
            if (debuffCollected)
            {
                debuffs.Remove(SDC);
                debuffCollected = false;
            }


            if (slow)
            {
                gameTimer.Interval = 200;
            }
            else
            {
                gameTimer.Interval = 10;
            }

            brickTime--;

            if (debuffs.Count != 0)
            {

                foreach (Debuff d in debuffs)
                {
                    d.Spawn();
                }
                for (int i = 0; i < debuffs.Count; i++)
                {
                    if (debuffs[i].y > this.Bottom)
                    {
                        debuffs.RemoveAt(i);
                    }
                }
            }

            //debuffs

            if (dB1)
            {
                duration1++;
                if (duration1 < 12)
                {
                    PictureBox vines = new PictureBox();
                    vines.Parent = this;
                    vines.Location = new Point(vineLocatoin, 0);
                    vines.SizeMode = PictureBoxSizeMode.StretchImage;
                    vines.Image = Properties.Resources.IvyVine;
                    //vines.BackColor = Color.Transparent;
                    vines.BringToFront();
                    debuff1.Add(vines);
                    vineLocatoin += 100;
                }
                else if (duration1 < 100)
                {
                    foreach (PictureBox p in debuff1)
                    {
                        p.Size = new Size(100, grow);
                    }
                    grow += 10;
                }
                else if (duration1 < 200)
                {
                    grow -= 10;
                    foreach (PictureBox p in debuff1)
                    {
                        p.Size = new Size(100, grow);
                    }
                }
                else
                {
                    duration1 = 0;
                    dB1 = false;
                    debuff1.Clear();
                    vineLocatoin = 130;
                }


            }

            if (dB2)
            {
                duration2++;
                if (duration2 < 2)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Ball newBall = new Ball(ball.x, ball.y, rand.Next(-18, 18), rand.Next(-18, 18), ball.size);
                        freakyBalls.Add(newBall);
                    }

                }

                if (duration2 > 200)
                {
                    freakyBalls.Clear();
                    dB2 = false;
                    duration2 = 0;
                }

                if (duration2 % 5 == 0 && drawBall == 1)
                {
                    drawTheBall = false;
                    drawBall = 0;
                }
                else if (duration2 % 5 == 0 && drawBall == 0)
                {
                    drawTheBall = true;
                    drawBall = 1;
                }


            }

            if (dB3)
            {
                //send to game over screen in future
                duration3++;
                
                if (duration3 > 60 && duration3 < 100)
                {
                    evilSkullMan.Parent = this;
                    evilSkullMan.Location = new Point((this.Width - evilSkullMan.Width) / 2, (this.Height - evilSkullMan.Height) / 2);
                    evilSkullMan.SizeMode = PictureBoxSizeMode.StretchImage;
                    evilSkullMan.Image = Properties.Resources.evilFace;
                    evilSkullMan.Size = new Size(700, 700);
                    evilSkullMan.BringToFront();
                }
                else if (duration3 > 100)
                {
                    duration3 = 0;
                    dB3 = false;
                    Application.Exit();
                }
                
            }

            if (dB4)
            {
                duration4++;
                if (duration4 < 3)
                {

                    whiteBoy.Parent = this;
                    whiteBoy.Location = new Point((this.Width - whiteBoy.Width) / 2, (this.Height - whiteBoy.Height) / 2);
                    whiteBoy.SizeMode = PictureBoxSizeMode.StretchImage;
                    whiteBoy.Image = Properties.Resources.WhiteBoy;
                    whiteBoy.BringToFront();
                    //play sound
                }
                else if (duration4 < 40 && duration4 > 20)
                {
                    whiteBoy.Visible = true;
                    whiteBoy.Size = new Size(100, 100);
                    whiteBoy.Location = new Point((this.Width - whiteBoy.Width) / 2, (this.Height - whiteBoy.Height) / 2);
                    //play sound
                }
                else if (duration4 < 200 && duration4 > 180)
                {
                    whiteBoy.Visible = true;
                    whiteBoy.Size = new Size(200, 200);
                    whiteBoy.Location = new Point((this.Width - whiteBoy.Width) / 2, (this.Height - whiteBoy.Height) / 2);
                    //play sound
                }
                else if (duration4 < 400 && duration4 > 380)
                {
                    whiteBoy.Visible = true;
                    whiteBoy.Size = new Size(700, 700);
                    whiteBoy.Location = new Point((this.Width - whiteBoy.Width) / 2, (this.Height - whiteBoy.Height) / 2);
                    //play sound
                }
                else
                {
                    whiteBoy.Visible = false;
                }

                if (duration4 > 400)
                {
                    dB4 = false;
                    duration4 = 0;
                }


            }

            if (dB5)
            {
                duration5++;
                if (duration5 < 1000)
                {
                    //mirror ball
                    mirroredBallX = this.Width - ball.x - ball.size;
                    //mirror paddle
                    mirroredPaddleX = this.Width - paddle.x - paddle.width;

                    #region regions
                    mirroredPaddleCircle.Reset();
                    mirroredLeftPaddleRegion.Dispose();
                    mirroredPaddleCircle.AddEllipse(mirroredPaddleX - 20, paddle.y, 40, 40);
                    mirroredLeftPaddleRegion = new Region(mirroredPaddleCircle);
                    mirroredLeftPaddleRegion.Exclude(new Rectangle(mirroredPaddleX - 20, paddle.y + 20, 40, 20));
                    mirroredLeftPaddleRegion.Exclude(new Rectangle(mirroredPaddleX, paddle.y, 20, 20));

                    mirroredPaddleCircle.Reset();
                    mirroredRightPaddleRegion.Dispose();
                    mirroredPaddleCircle.AddEllipse(mirroredPaddleX + paddle.width - 20, paddle.y, 40, 40);
                    mirroredRightPaddleRegion = new Region(mirroredPaddleCircle);
                    mirroredRightPaddleRegion.Exclude(new Rectangle(mirroredPaddleX + paddle.width - 20, paddle.y + 20, 40, 20));
                    mirroredRightPaddleRegion.Exclude(new Rectangle(mirroredPaddleX + paddle.width - 20, paddle.y, 20, 20));
                    #endregion
                }
                else
                {
                    dB5 = false;
                    duration5 = 0;
                }

            }


            #endregion

            brickTime--;

            Refresh();
        }

        private float derivitive()
        {
            //bounce curve modelled in desmos, which can be found here: "https://www.desmos.com/calculator/emiewzq0xk"
            using (Graphics e = this.CreateGraphics())
            {
                if (ball.x < paddle.x + (paddle.width / 2)) //checks for left collision
                {
                    checkRegions[0] = ballRegion;
                    checkRegions[1] = leftPaddleRegion;
                    checkRegions[0].Intersect(checkRegions[1]);

                    if (!checkRegions[0].IsEmpty(e))
                    {
                        float x = ball.x + ball.size - paddle.x;
                        leftCircleCollision = true;

                        slope = (float)(-x / Math.Sqrt(400 - Math.Pow(x, 2)));
                        return slope;
                    }
                }
                else //checks for right collision
                {
                    checkRegions[0] = ballRegion;
                    checkRegions[1] = rightPaddleRegion;
                    checkRegions[0].Intersect(checkRegions[1]);

                    if (!checkRegions[0].IsEmpty(e))
                    {
                        float x = ball.x - (paddle.x + paddle.width);
                        rightCircleCollision = true;

                        slope = (float)(-x / Math.Sqrt(400 - Math.Pow(x, 2)));
                        return slope;
                    }
                }
                leftCircleCollision = false;
                rightCircleCollision = false;
                return 1;
            }
        }
        private void updateCurve()
        {
            paddleCircle.Reset();
            leftPaddleRegion.Dispose();
            paddleCircle.AddEllipse(paddle.x - 20, paddle.y, 40, 40);
            leftPaddleRegion = new Region(paddleCircle);
            leftPaddleRegion.Exclude(new Rectangle(paddle.x - 20, paddle.y + 20, 40, 20));
            leftPaddleRegion.Exclude(new Rectangle(paddle.x, paddle.y, 20, 20));

            paddleCircle.Reset();
            rightPaddleRegion.Dispose();
            paddleCircle.AddEllipse(paddle.x + paddle.width - 20, paddle.y, 40, 40);
            rightPaddleRegion = new Region(paddleCircle);
            rightPaddleRegion.Exclude(new Rectangle(paddle.x + paddle.width - 20, paddle.y + 20, 40, 20));
            rightPaddleRegion.Exclude(new Rectangle(paddle.x + paddle.width - 20, paddle.y, 20, 20));
        }

        public void updateBallStorage()
        {
            ballCircle.Reset();
            ballRegion.Dispose();

            ballCircle.AddRectangle(new RectangleF(ball.x, ball.y, ball.size, ball.size));
            ballRegion = new Region(ballCircle);
        }

        public void OnEnd()
        {
            // Goes to the game over screen
            Form form = this.FindForm();
            MenuScreen ps = new MenuScreen();

            ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);

            Cursor.Show();
            form.Controls.Add(ps);
            form.Controls.Remove(this);

        }

        private void GameScreen_MouseDown(object sender, MouseEventArgs e)
        {
            mouseMoving = true;
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            UIPaint.PaintTransRectangle(e.Graphics, Color.White, new Rectangle(0, 0, 128, this.Height), 50);
            UIPaint.PaintTransRectangle(e.Graphics, Color.White, new Rectangle(this.Width - 128, 0, 128, this.Height), 50);

            UIPaint.PaintText(e.Graphics, "Level 0", 24, new Point(this.Width - 120, 90), Color.Goldenrod);
            Image heartImage = Properties.Resources.heart1;
            Point lifePos = new Point(this.Width - heartImage.Width - 25, 25);
            switch (lives)
            {

                case 1:
                    e.Graphics.DrawImage(Properties.Resources.heart1, lifePos);
                    break;
                case 2:
                    e.Graphics.DrawImage(Properties.Resources.heart2, lifePos);
                    break;
                case 3:
                    e.Graphics.DrawImage(Properties.Resources.heart3, lifePos);
                    break;
                case 4:
                    e.Graphics.DrawImage(Properties.Resources.heart4, lifePos);
                    break;
                case 5:
                    e.Graphics.DrawImage(Properties.Resources.heart5, lifePos);
                    break;
                case 6:
                    e.Graphics.DrawImage(Properties.Resources.heart6, lifePos);
                    break;
                default:
                    e.Graphics.DrawImage(Properties.Resources.heart6, lifePos);
                    break;
            }
            UIPaint.PaintText(e.Graphics, lives + "", 24, new Point(this.Width - 55, 50), Color.Red);
            Font myFont = new Font("Chiller", scoreSize, FontStyle.Bold);
            SizeF textSize = e.Graphics.MeasureString(score.score + "", myFont);
            
            if (scoreDirection == 1)
            {
                scoreAngle++;
            }
            else
            {
                scoreAngle--;
            }
            if (scoreAngle > 10)
            {
                scoreDirection *= -1;
            }
            if (scoreAngle < -10)
            {
                scoreDirection *= -1;
            }


            // Draws paddle
            paddleBrush.Color = paddle.colour;
            e.Graphics.FillRectangle(paddleBrush, paddle.x, paddle.y, paddle.width, paddle.height);

            e.Graphics.FillRegion(Brushes.White, leftPaddleRegion);
            e.Graphics.FillRegion(Brushes.White, rightPaddleRegion);

            Block.PaintBlocks(e.Graphics, blocks);

            foreach (Debuff d in debuffs)
            {
                if (d.y < this.Bottom)
                {
                    e.Graphics.DrawRectangle(Pens.White, d.x, d.y, 10, 10);
                    Brush brush = new SolidBrush(d.color);
                    e.Graphics.FillRectangle(brush, d.x, d.y, 10, 10);
                }
            }

            // Draws ball

            if (drawTheBall)
            {
                foreach (Ball b in freakyBalls)
                {
                    e.Graphics.FillEllipse(ballBrush, b.x, b.y, b.size, b.size);
                }
            }


            e.Graphics.FillEllipse(ballBrush, ball.x, ball.y, ball.size, ball.size);

            if (dB5)
            {
                e.Graphics.FillEllipse(ballBrush, mirroredBallX, ball.y, ball.size, ball.size);

                //fix paddle shape

                e.Graphics.FillRectangle(paddleBrush, mirroredPaddleX, paddle.y, paddle.width, paddle.height);
                e.Graphics.FillRegion(paddleBrush, mirroredLeftPaddleRegion);
                e.Graphics.FillRegion(paddleBrush, mirroredRightPaddleRegion);
            }

            UIPaint.PaintTextRotate(e.Graphics, score.score + "", scoreSize, new Point(this.Width / 2, this.Height / 2 - 360), Color.Red, scoreAngle, new Point((int)textSize.Width / 2, (int)textSize.Height / 2));


            // test
            // e.Graphics.DrawRectangle(Pens.White, ball.x, ball.y, ball.size, ball.size);


            //testing

            //foreach (Block block in blocks)
            //{
            //    e.Graphics.DrawRectangle(Pens.RoyalBlue, block.hitBox);
            //}

            //e.Graphics.DrawRectangle(Pens.Red, Convert.ToInt16(ball.x), Convert.ToInt16(ball.y) + 5, 1, Convert.ToInt16(ball.size) - 10);
            //e.Graphics.DrawRectangle(Pens.Red, Convert.ToInt16(ball.x) + Convert.ToInt16(ball.size), Convert.ToInt16(ball.y) + 5, 1, Convert.ToInt16(ball.size) - 10);
            //e.Graphics.DrawRectangle(Pens.Red, Convert.ToInt16(ball.x) + 5, Convert.ToInt16(ball.y), Convert.ToInt16(ball.size) - 10, 1);
            //e.Graphics.DrawRectangle(Pens.Red, Convert.ToInt16(ball.x) + 5, Convert.ToInt16(ball.y) + Convert.ToInt16(ball.size), Convert.ToInt16(ball.size) - 10, 1);

            ////Rectangle bRl = new Rectangle(Convert.ToInt16(ball.x), Convert.ToInt16(ball.y) + 10, 1, Convert.ToInt16(ball.size) - 10);
            ////Rectangle bRr = new Rectangle(Convert.ToInt16(ball.x) + Convert.ToInt16(ball.size), Convert.ToInt16(ball.y) + 10, 1, Convert.ToInt16(ball.size) - 10);
            ////Rectangle bRt = new Rectangle(Convert.ToInt16(ball.x) + 10, Convert.ToInt16(ball.y), Convert.ToInt16(ball.size) - 10, 1);
            ////Rectangle bRb = new Rectangle(Convert.ToInt16(ball.x) + 10, Convert.ToInt16(ball.y) + Convert.ToInt16(ball.size), Convert.ToInt16(ball.size) - 10, 1);
            ///

            for (int i = 0; i < comboAdds.Count(); i++)
            {
                if (comboAdds.Count() != 0)
                {
                    UIPaint.PaintTextTrans(e.Graphics, comboAdds[i].text, 30, comboAdds[i].drawPoint, Color.Red, comboAdds[i].transparency);
                    comboAdds[i].transparency -= 7;
                    if (comboAdds[i].transparency <= 0)
                    {
                        comboAdds.Remove(comboAdds[i]);
                    }
                }
            }
        }
    }
}

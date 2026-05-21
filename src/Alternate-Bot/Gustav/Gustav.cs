using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Gustav : Bot
{
    private const double EffectiveFiringRange = 350; 
    private const double PointBlankRange = 90;       

    private double moveAmountWidth = 0;
    private double moveAmountHeight = 0;
    private int currentSide = 0; 
    private bool isInitialized = false;
    private bool isCalibrated = false;

    private int stuckTicks = 0;

    private double targetX = -1;
    private double targetY = -1;
    private bool trackingTarget = false;
    private int trackScanTicks = 0;

    static void Main(string[] args)
    {
        new Gustav().Start();
    }

    public Gustav() : base(BotInfo.FromFile("Gustav.json")) { }

    private void UpdateWeaponSystem()
    {
        if (!trackingTarget)
        {
            SetTurnRadarLeft(360);
        }
        else
        {
            double absoluteDirection = DirectionTo(targetX, targetY);
            double currentDistance = DistanceTo(targetX, targetY);

            double gunBearing = NormalizeRelativeAngle(absoluteDirection - GunDirection);
            SetTurnGunLeft(gunBearing);

            double radarBearing = NormalizeRelativeAngle(absoluteDirection - RadarDirection);
            double microSweep = (trackScanTicks % 2 == 0) ? 6 : -6;
            SetTurnRadarLeft(radarBearing + microSweep);

            if (currentDistance <= EffectiveFiringRange)
            {
                double bulletPower = (currentDistance <= PointBlankRange) ? 3.0 : 1.0;
                Fire(bulletPower);
            }

            trackScanTicks++;
            if (trackScanTicks >= 20)
            {
                trackingTarget = false;
                trackScanTicks = 0;
            }
        }
    }

    public override void Run()
    {
        BodyColor   = Color.FromArgb(0x1A, 0x1A, 0x1A);
        TurretColor = Color.FromArgb(0xD4, 0xAF, 0x37);
        RadarColor  = Color.FromArgb(0x00, 0xFF, 0x66);
        BulletColor = Color.FromArgb(0xFF, 0x33, 0x33);
        ScanColor   = Color.FromArgb(0x00, 0xFF, 0x66);

        while (IsRunning)
        {
            if (!isInitialized)
            {
                moveAmountWidth = ArenaWidth * 0.55;
                moveAmountHeight = ArenaHeight * 0.55;
                isInitialized = true;
            }

            if (!isCalibrated)
            {
                double borderPaddingX = (ArenaWidth - moveAmountWidth) / 2;
                double borderPaddingY = (ArenaHeight - moveAmountHeight) / 2;

                double minXBound = borderPaddingX;
                double maxXBound = ArenaWidth - borderPaddingX;
                double minYBound = borderPaddingY;
                double maxYBound = ArenaHeight - borderPaddingY;

                double distToLeft   = Math.Abs(X - minXBound);
                double distToRight  = Math.Abs(X - maxXBound);
                double distToBottom = Math.Abs(Y - minYBound);
                double distToTop    = Math.Abs(Y - maxYBound);

                double minTarget = Math.Min(Math.Min(distToLeft, distToRight), Math.Min(distToBottom, distToTop));

                if (minTarget == distToLeft)
                {
                    TurnRight(NormalizeRelativeAngle(180 - Direction));
                    Forward(distToLeft);
                    TurnRight(NormalizeRelativeAngle(90 - Direction));
                    currentSide = 0; 
                }
                else if (minTarget == distToRight)
                {
                    TurnRight(NormalizeRelativeAngle(0 - Direction));
                    Forward(distToRight);
                    TurnRight(NormalizeRelativeAngle(270 - Direction));
                    currentSide = 2; 
                }
                else if (minTarget == distToBottom)
                {
                    TurnRight(NormalizeRelativeAngle(270 - Direction));
                    Forward(distToBottom);
                    TurnRight(NormalizeRelativeAngle(180 - Direction));
                    currentSide = 3; 
                }
                else if (minTarget == distToTop)
                {
                    TurnRight(NormalizeRelativeAngle(90 - Direction));
                    Forward(distToTop);
                    TurnRight(NormalizeRelativeAngle(0 - Direction));
                    currentSide = 1; 
                }

                isCalibrated = true;
            }

            UpdateWeaponSystem();

            double targetDistance = (currentSide == 0 || currentSide == 2) ? moveAmountWidth : moveAmountHeight;
            SetForward(targetDistance);

            while (DistanceRemaining != 0)
            {
                UpdateWeaponSystem();

                if (Math.Abs(Speed) < 0.1)
                {
                    stuckTicks++;
                }
                else
                {
                    stuckTicks = 0;
                }

                if (stuckTicks > 5)
                {
                    Back(60); 
                    stuckTicks = 0;
                    break; 
                }

                Go();
            }

            TurnRight(90);

            currentSide = (currentSide + 1) % 4;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (!isCalibrated) return;

        targetX = e.X;
        targetY = e.Y;
        
        trackingTarget = true;
        trackScanTicks = 0; 
    }
}
using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Nomad : Bot
{
    private const double EffectiveFiringRange = 425;
    private const double PointBlankRange = 110;       

    private double[] waypointX = new double[4];
    private double[] waypointY = new double[4];
    private int currentWaypointIndex = 0;
    private bool trackInitialized = false;

    private double targetX = -1;
    private double targetY = -1;
    private bool trackingTarget = false;
    private int trackScanTicks = 0;

    private int stuckTicks = 0;

    static void Main(string[] args)
    {
        new Nomad().Start();
    }

    public Nomad() : base(BotInfo.FromFile("Nomad.json")) { }

    private void InitializeTrackRails()
    {
        double inset = 75; 
        
        waypointX[0] = inset;
        waypointY[0] = inset;

        waypointX[1] = ArenaWidth - inset;
        waypointY[1] = inset;

        waypointX[2] = ArenaWidth - inset;
        waypointY[2] = ArenaHeight - inset;

        waypointX[3] = inset;
        waypointY[3] = ArenaHeight - inset;

        trackInitialized = true;
    }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0x00, 0xFF);   
        TurretColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
        RadarColor = Color.FromArgb(0xFF, 0x00, 0x00);  
        BulletColor = Color.FromArgb(0x00, 0xFF, 0xFF); 
        ScanColor = Color.FromArgb(0x00, 0xFF, 0xFF);
		
        while (IsRunning)
        {
            if (!trackInitialized)
            {
                InitializeTrackRails();
            }

            double nextX = waypointX[currentWaypointIndex];
            double nextY = waypointY[currentWaypointIndex];
            double distanceToWaypoint = DistanceTo(nextX, nextY);

            if (distanceToWaypoint < 45)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % 4;
                nextX = waypointX[currentWaypointIndex];
                nextY = waypointY[currentWaypointIndex];
            }

            double angleToWaypoint = DirectionTo(nextX, nextY);
            double steeringBearing = NormalizeRelativeAngle(angleToWaypoint - Direction);

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
                SetTurnLeft(90);
                SetForward(80); 
            }
            else
            {
                SetTurnLeft(steeringBearing);
                SetForward(115); 
            }

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
                    double bulletPower = (currentDistance <= PointBlankRange) ? 3.0 : 0.5;
                    Fire(bulletPower);
                }

                trackScanTicks++;
                if (trackScanTicks >= 20)
                {
                    trackingTarget = false;
                    trackScanTicks = 0;
                }
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        targetX = e.X;
        targetY = e.Y;
        trackingTarget = true;
    }
}
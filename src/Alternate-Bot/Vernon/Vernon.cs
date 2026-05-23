using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Vernon : Bot
{   

    private double targetSpeed = 0;
    private double targetDirection = 0;

    private double targetX = -1;
    private double targetY = -1;
    private bool trackingTarget = false;
    private int trackScanTicks = 0;

    private const double EffectiveFiringRange = 350;
    private const double PointBlackRange = 90;

    private int moveDirection = 1; 


    static void Main(string[] args)
    {
        new Vernon().Start();
    }

    Vernon() : base(BotInfo.FromFile("Vernon.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0x80, 0x80);
        TurretColor = Color.FromArgb(0x1F, 0xA2, 0xA5);
        RadarColor = Color.FromArgb(0x38, 0x84, 0x97);
        BulletColor = Color.FromArgb(0x38, 0xA2, 0x97);
        ScanColor = Color.FromArgb(0x00, 0x91, 0x92);
        TracksColor = Color.FromArgb(0x00, 0x40, 0x40);
        GunColor = Color.FromArgb(0x1F, 0xA2, 0xA5);    

        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

   
            while (IsRunning) {

                if (!trackingTarget) {

                    SetTurnRadarLeft(360);
                    SetForward(100 * moveDirection);

                } else {
                    
                    double absoluteDirection = DirectionTo(targetX, targetY);
                    double gunBearing = NormalizeRelativeAngle(absoluteDirection - GunDirection + 5);
                    SetTurnGunLeft(gunBearing);

                    double radarBearing = NormalizeRelativeAngle(absoluteDirection - RadarDirection);
                    double microSweep = (trackScanTicks % 2 == 0) ? 6 : -6;
                    SetTurnRadarLeft (radarBearing + microSweep);

                    double jarak = DistanceTo(targetX, targetY);

                    double bearingFromBot = NormalizeRelativeAngle(BearingTo(targetX, targetY) - Direction);
                    if (jarak <= 200){  
                    SetTurnRight(bearingFromBot + 200);
                    SetForward(150 * moveDirection);
                    } else if (jarak > 170) {
                        SetTurnRight(bearingFromBot + 60);
                        SetForward(120 * moveDirection);
                    } else if (jarak > 150) {
                        SetTurnRight(bearingFromBot + 120);
                        SetForward(120 * moveDirection);
                    } else if (jarak > 100) {
                        double matchingDirection = NormalizeRelativeAngle(targetDirection - Direction);
                        SetTurnRight(matchingDirection + 90);
                        SetForward(targetSpeed + 5 * moveDirection);
                    } else {
                        SetTurnRight(bearingFromBot);
                        SetForward(300 * moveDirection);
                    }
                    
                        if (jarak <= PointBlackRange) {
                            SetFire(3);

                            } else {
                            SetFire(1);
                            }         
                }

                    trackScanTicks++;
                if (trackScanTicks >= 20) {

                    trackingTarget = false;
                    trackScanTicks = 0;
                }

                Go();
            }

            
    }

    public override void OnScannedBot(ScannedBotEvent e) {
       
        targetX = e.X;           
        targetY = e.Y;           
        targetSpeed = e.Speed;
        targetDirection = e.Direction;
        trackingTarget = true;   
        trackScanTicks = 0;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        moveDirection *= -1;
    }

    public override void OnHitWall(HitWallEvent e)
    {
        moveDirection *= -1;
    }
}
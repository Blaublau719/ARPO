using UnityEngine;
using UnityEngine.UI;
using System;
//using NUnit.Framework;
using System.Collections.Generic;
using UnityEditorInternal;
//using System.Numerics;

public class main : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    public BackgroundData m_beforelastFrameData = new BackgroundData();
    public bool Hiplower;  //hip lower than knee
    public bool KneeFoot;  //Keep knee in line with foot
    public bool HeelFloor; //Keep the heels on the floor
    public bool StraightSpine; //keep spine straight
    public bool Midfoot;  //Keep barbell over midfoot
    public bool EvenGrips;
    public bool Knee30;
    public float Hlthreshold;


    List<BackgroundData> FrameList = new List<BackgroundData>();
    List<float> HipY = new List<float>();
    List<float> CSr = new List<float>();

    bool FrameaddTag = false; //not yet added any
    bool HLaddTag = false;
    bool EGaddTag = false;
    float HFTag = 0;
    float SSTag = 0;
    float BMTag = 0;
    float SquatCounter = 0;

    float MaxSpineLength = 0;
    float InitHeelYRight = 0;
    float InitHeelYLeft = 0;
    float InitHipHigh = 1000;
    float InitHipLow = 0;
    float initRelY = 0;

    public Text HL;
    public Text HF;
    public Text SS;
    public Text BM;
    public Text EG;
    public Text LS;//encourage word
    public Text CS;
    public Text K30;
    public Text Counter;
    public Text HIPP;
    public Text CSrecord;

    float HLScore = 0;  //5 sets, 4 squats each, add on 5 points if hit one true
    float HFScore = 0;
    float SSScore = 0;
    float BMScore = 0;
    float EGScore = 0;
    
    float LastScore = 0;
    float CorrectScore = 0;

    void Start()
    {
        //tracker ids needed for when there are two trackers
        const int TRACKER_ID = 0;
        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);
    }

    void Update() //Update function is called in every frame
    {
        if (m_skeletalTrackingProvider.IsRunning)
        {
            if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
            {
                if (m_lastFrameData.NumOfBodies != 0)
                {
                    m_tracker.GetComponent<TrackerHandler>().updateTracker(m_lastFrameData);
                    MotionAnalysis();
                }
            }
        }
        m_beforelastFrameData = m_lastFrameData;  //store current frame data
    }

    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }

    void MotionAnalysis()
    {   // Assertion for a complete squat
        //First, get inital hip.Y at heighest and lowest (pelvis0)
        float CurrentHipY = m_lastFrameData.Bodies[0].JointPositions3D[0].Y;
        float LastHipY = m_beforelastFrameData.Bodies[0].JointPositions3D[0].Y;
        if (CurrentHipY > InitHipLow)  //find ankle lowest point
        {
            InitHipLow= CurrentHipY;
        }
        if (CurrentHipY < InitHipHigh)  //find ankle highest point
        {
            InitHipHigh = CurrentHipY;
        }
        //Next, collect frames for one squat. If hip lower than highest, include current frame.
        //HIPP.text = InitHipHigh.ToString();
        HipY.Add(CurrentHipY);
        //HIPP.text = CurrentHipY.ToString();
        //HIPP.text = HipY.ToString();
        //HIPP.text = (CurrentHipY-InitHipHigh).ToString() + (LastHipY - InitHipHigh).ToString();
        if (CurrentHipY > InitHipHigh)
        {
            if (CurrentHipY > LastHipY)
            {
                FrameaddTag = false;
            }
            FrameList.Add(m_beforelastFrameData);
            //CurrentHipY - InitHipHigh > 0.04 && CurrentHipY - InitHipHigh < 0.06 && LastHipY > 0.08
            if ((CurrentHipY - InitHipHigh < 0.085) && (LastHipY - InitHipHigh > 0.09) && FrameaddTag == false)
            {
                SquatCounter += 1;
                FrameaddTag = true;
                //FrameAnalyzer(FrameList);
                int length = FrameList.Count;
                if (HFTag > length)
                {
                    HFScore = 17; 
                } else
                { 
                    HFScore = 17 * HFTag / length; 
                }

                if (SSTag > length)
                {
                    SSScore = 25;
                }
                else
                {
                    SSScore = 25 * SSTag / length;
                }

                if (BMTag > length)
                {
                    BMScore = 25;
                }
                else
                {
                    BMScore = 25 * BMTag / length;
                }

                LastScore = CorrectScore;
                LS.text = LastScore.ToString();
                CSr.Add(CorrectScore);
                CSrecord.text = string.Join("  ", CSr);

                CorrectScore = HLScore + HFScore + SSScore + BMScore + EGScore;
                if (CorrectScore < 70)
                {
                    LS.text = "You can do better";
                } else if (CorrectScore < 80){
                    LS.text = "Good";
                } else if (CorrectScore < 95){
                    LS.text = "Very Good";
                } else if (CorrectScore < 100){
                    LS.text = "Excellent!";
                } 
                    //+ HFScore + SSScore + BMScore + EGScore
                    CS.text = CorrectScore.ToString("0.00");

                HF.text = "";
                HLaddTag = false;
                EGaddTag = false;
                HFTag = 0;
                SSTag = 0;
                BMTag = 0;
                HLScore = 0;  //5 sets, 4 squats each, add on 5 points if hit one true
                HFScore = 0;
                SSScore = 0;
                BMScore = 0;
                EGScore = 0;
                FrameList.Clear();
            }

        }
        Counter.text = SquatCounter.ToString();


        // HL
        //Hip lower than knee
        //HIPP.text = (m_lastFrameData.Bodies[0].JointPositions3D[0].Y - m_lastFrameData.Bodies[0].JointPositions3D[19].Y).ToString();
        if (m_lastFrameData.Bodies[0].JointPositions3D[0].Y > m_lastFrameData.Bodies[0].JointPositions3D[19].Y - 0.1 && m_lastFrameData.Bodies[0].JointPositions3D[0].Y > m_lastFrameData.Bodies[0].JointPositions3D[23].Y - 0.1)
        {
            Hiplower = true;
            HL.text = "Excellent!";
            if (HLaddTag == false)
            {
                HLScore += 25;
            }
            HLaddTag = true;
        }
        else
        {
            Hiplower = false;
            HL.text = "Lower!";
            //HL.text = Hiplower.ToString();
        }


        // HF
        //Keep heel on the floor 
        float CurrHeelYRight = m_lastFrameData.Bodies[0].JointPositions3D[24].Y;
        float CurrHeelYLeft = m_lastFrameData.Bodies[0].JointPositions3D[20].Y;
        float CurrHeelMid = CurrHeelYLeft + CurrHeelYRight;
        float currMidfootY = Math.Abs(m_beforelastFrameData.Bodies[0].JointPositions3D[21].Y + m_beforelastFrameData.Bodies[0].JointPositions3D[25].Y) / 2;
        float currRelY = Math.Abs(CurrHeelMid - currMidfootY);
        //float FeetYRight = m_lastFrameData.Bodies[0].JointPositions3D[25].Y;
        //float FeetYLeft = m_lastFrameData.Bodies[0].JointPositions3D[21].Y;
        if (CurrHeelYRight > InitHeelYRight)  //find ankle lowest point
        {
            InitHeelYRight = CurrHeelYRight;
        }
        if (CurrHeelYLeft > InitHeelYLeft)  //find ankle lowest point
        {
            InitHeelYLeft = CurrHeelYLeft;
        }
        if (currRelY > initRelY)  //find ankle lowest point
        {
            initRelY = currRelY;
        }

        HIPP.text = currRelY.ToString();
        //HIPP.text = (InitHeelYRight - CurrHeelYRight).ToString() + InitHeelYRight.ToString();
        //float HeelMovement = m_lastFrameData.Bodies[0].JointPositions3D[20].Y + m_lastFrameData.Bodies[0].JointPositions3D[24].Y - m_beforelastFrameData.Bodies[0].JointPositions3D[20].Y - m_beforelastFrameData.Bodies[0].JointPositions3D[24].Y;
        //InitHeelYLeft - CurrHeelYLeft > 0.12 || InitHeelYRight - CurrHeelYRight > 0.12;
        if (currRelY < Hlthreshold && m_beforelastFrameData.Bodies[0].JointPositions3D[0].Y > InitHipHigh + 0.1)  //if current ankle position higher than lowest position, then considered as heel not on the floor
        {
            HeelFloor = false;
            HF.text = "Stick heels on the floor";
        }
        else
        {
            HeelFloor = true;  //Heel Movement exists, heel was not on the floor
            HF.text = "";
            HFTag += 1;
        }


        // SS
        //Keep Spine Straight, keep the staight line distance between neck and pelvis the same
        float PelvisY = m_lastFrameData.Bodies[0].JointPositions3D[0].Y;
        float PelvisX = m_lastFrameData.Bodies[0].JointPositions3D[0].X;
        float NeckY = m_lastFrameData.Bodies[0].JointPositions3D[3].Y;
        float NeckX = m_lastFrameData.Bodies[0].JointPositions3D[3].X;
        float BPelvisY = m_beforelastFrameData.Bodies[0].JointPositions3D[0].Y;
        float BPelvisX = m_beforelastFrameData.Bodies[0].JointPositions3D[0].X;
        float BNeckY = m_beforelastFrameData.Bodies[0].JointPositions3D[3].Y;
        float BNeckX = m_beforelastFrameData.Bodies[0].JointPositions3D[3].X;

        float NeckPelvisDistance = (PelvisY - NeckY) * (PelvisY - NeckY) + (PelvisX - NeckX) * (PelvisX- NeckX);
        float BNeckPelvisDistance = (BPelvisY - BNeckY) * (BPelvisY - BNeckY) + (BPelvisX - BNeckX) * (BPelvisX - BNeckX);
        //HIPP.text = (MaxSpineLength - NeckPelvisDistance).ToString();
        if (Math.Max(NeckPelvisDistance, BNeckPelvisDistance) > MaxSpineLength)
        {
            MaxSpineLength = Math.Max(NeckPelvisDistance, BNeckPelvisDistance);
        }

        if (MaxSpineLength - NeckPelvisDistance > 0.15)
        {
            StraightSpine = false;
            SS.text = "Keep spine straight";
        }
        else
        {
            StraightSpine = true;
            SS.text = "";
            SSTag += 1;
        }


        // BM
        //Keep barbell over midfoot
        float MidHandZ = (m_lastFrameData.Bodies[0].JointPositions3D[8].Z + m_lastFrameData.Bodies[0].JointPositions3D[15].Z) / 2;
        float MidFeetZ = (m_lastFrameData.Bodies[0].JointPositions3D[20].Z + m_lastFrameData.Bodies[0].JointPositions3D[21].Z) / 2 + (m_lastFrameData.Bodies[0].JointPositions3D[24].Z + m_lastFrameData.Bodies[0].JointPositions3D[25].Z) / 2;
        //HIPP.text = (Math.Abs(MidFeetZ - MidHandZ)).ToString(); //if lean forward >2.3, lean backward<2.3
        if (Math.Abs(MidFeetZ-MidHandZ-2.3) < 0.25)
        {
            Midfoot = true;
            BM.text = "";
            BMTag += 1;
        }
        else
        {
            Midfoot = false;
            BM.text = "Keep balance";
        }



        // EG
        //Even grips on barbell
        float MidHandX = (m_lastFrameData.Bodies[0].JointPositions3D[8].X + m_lastFrameData.Bodies[0].JointPositions3D[15].X) / 2;
        float MidFeetX = (m_lastFrameData.Bodies[0].JointPositions3D[20].X + m_lastFrameData.Bodies[0].JointPositions3D[21].X) / 2 + (m_lastFrameData.Bodies[0].JointPositions3D[24].X + m_lastFrameData.Bodies[0].JointPositions3D[25].X) / 2;
        float leftGrip = Math.Abs(m_lastFrameData.Bodies[0].JointPositions3D[8].X - NeckX); //0.3
        float rightGrip = Math.Abs(m_lastFrameData.Bodies[0].JointPositions3D[15].X - NeckX); //0.1
        //HIPP.text = leftGrip.ToString() + rightGrip.ToString() + MidFeetX.ToString();
        //Math.Abs(MidFeetX - MidHandX - 0.12) < 0.32 && Math.Abs(MidFeetX - MidHandX - 0.12) > 0.24
        if (Math.Abs(rightGrip - leftGrip) < 0.05)
        {
            EvenGrips = true;
            EG.text = "Excellent!";
            if (EGaddTag == false)
            {
                EGScore += 8;
            }
            EGaddTag = true;
        }
        else
        {
            EvenGrips = false;
            if (rightGrip - 0.05 > leftGrip)
            {
                EG.text = "Right hand ←←←";
            }
            if (rightGrip + 0.05 < leftGrip)
            {
                EG.text = "Right hand →→→";
            }

        }



        // Knee30
        //髋角
        Vector2 HipNeck = new Vector2(m_lastFrameData.Bodies[0].JointPositions3D[3].Z - m_lastFrameData.Bodies[0].JointPositions3D[0].Z, m_lastFrameData.Bodies[0].JointPositions3D[3].Y - m_lastFrameData.Bodies[0].JointPositions3D[0].Y);
        double HipAngle = Math.Atan2(HipNeck.x, HipNeck.y);
        HipAngle = HipAngle * (180 / Math.PI);
        //Vector2 UnitY = new Vector2(0, 1);
        //double leftKneeDegree = Math.Acos(Vector2.Dot(leftKneeVector, UnitY));
        double K30angle = Math.Atan2(m_lastFrameData.Bodies[0].JointPositions3D[23].Z, m_lastFrameData.Bodies[0].JointPositions3D[23].X);
        //double HipAngle = Math.Atan2(m_lastFrameData.)
        K30angle = K30angle * (180 / Math.PI);
        K30.text = HipAngle.ToString(); //initial 95, start sqauting 100, if number not changing much then fail??

        
    }

    private void print(float initHipHigh, float initHipLow)
    {
        throw new NotImplementedException();
    }

    void FrameAnalyzer(List<BackgroundData> FrameList)
    {
        foreach (BackgroundData Frame in FrameList)
        {
            //Hip lower than knee
            if (Frame.Bodies[0].JointPositions3D[0].Y > Frame.Bodies[0].JointPositions3D[19].Y && Frame.Bodies[0].JointPositions3D[0].Y > Frame.Bodies[0].JointPositions3D[23].Y)
            {
                Hiplower = true;
                HL.text = "✔";
                HLScore += 5;
            }
            else
            {
                Hiplower = false;
                HL.text = "✘";
                //HL.text = Hiplower.ToString();
            }
        }
    }


}

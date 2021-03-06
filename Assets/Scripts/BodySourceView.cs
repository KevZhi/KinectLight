using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using System.Linq;
using System;

public class BodySourceView : MonoBehaviour
{
    private Vector2 leftLastValue;
    private Vector2 rightLastValue;
    private Vector2 lastvleft = new Vector2 (0,0);
    private Vector3 lastvright = new Vector3();
    private float leftLastAngle = -1;
    private float rightLastAngle = -1;
    public Slider lumislider1;
    public Slider sliderx1;
    public Slider slidery1;
    public Slider lumislider2;
    public Slider sliderx2;
    public Slider slidery2;
    public Slider Ch1Bar;
    public Slider Ch2Bar;
    public Slider Ch3Bar;
    public Slider Ch4Bar;
    public Slider Ch5Bar;
    public Slider Ch6Bar;
    public Slider DistanceSlider;
    public Light lightl;
    public Light lightr;
    public Text LLumi;
    public Text RLumi;
    public Text LOrit;
    public Text ROrit;
    public Text DistanceText;
    public Text Ch1Num;
    public Text Ch2Num;
    public Text Ch3Num;
    public Text Ch4Num;
    public Text Ch5Num;
    public Text Ch6Num;
    public Text DMXText2;
    public Text DMXText16;
    public Canvas DMXCanvas; 
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public Slider smoothFactor;
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private List<float> leftAngleList;
    private List<float> rightAngleList;
    public Text SliderValue;

 //   private Vector3 vleftrad = new Vector3(0, 0, 0);
    private List<Vector3> leftvlist;
    private List<Vector3> rightvlist;
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    private ulong nearest_body;
    private float near_Z;

    private void Start()
    {
    lumislider1= GameObject.Find("LumiSlider1").GetComponent<Slider>();
    sliderx1= GameObject.Find("SliderX1").GetComponent<Slider>();
    slidery1= GameObject.Find("SliderY1").GetComponent<Slider>();
    lumislider2= GameObject.Find("LumiSlider2").GetComponent<Slider>();
    sliderx2= GameObject.Find("SliderX2").GetComponent<Slider>();
    slidery2= GameObject.Find("SliderY2").GetComponent<Slider>();
    lightl = GameObject.Find("lightl").GetComponent<Light>();
    lightr= GameObject.Find("lightr").GetComponent<Light>();
    LLumi= GameObject.Find("LLumi").GetComponent<Text>();
    RLumi = GameObject.Find("RLumi").GetComponent<Text>();
    LOrit = GameObject.Find("LOrit").GetComponent<Text>();
    ROrit = GameObject.Find("ROrit").GetComponent<Text>();
        DMXCanvas = GameObject.Find("DMXCanvas").GetComponent<Canvas>();
        DMXCanvas.gameObject.SetActive(false);


    leftAngleList = new List<float>();
    rightAngleList = new List<float>();
    leftvlist = new List<Vector3>();
     rightvlist = new List<Vector3>();
     SliderValue = GameObject.Find("smoothslidervalue").GetComponent<Text>();
      
    }
    
    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null) continue; //如获取不到人体ID则跳出循环
            if (body.IsTracked) trackedIds.Add(body.TrackingId);//如获取到人体ID则加入可访问的ID列表
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);  
                }

                Kinect.Joint leftWrist = body.Joints[Kinect.JointType.WristLeft];
                Kinect.Joint leftElbow = body.Joints[Kinect.JointType.ElbowLeft];
                Kinect.Joint leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
                Kinect.Joint rightWrist = body.Joints[Kinect.JointType.WristRight];
                Kinect.Joint rightElbow = body.Joints[Kinect.JointType.ElbowRight];
                Kinect.Joint rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
 
                float leftangle = Vector3.Angle((GetVector3FromJoint(leftWrist)) - (GetVector3FromJoint(leftElbow)), (GetVector3FromJoint(leftShoulder)) - (GetVector3FromJoint(leftElbow)));
                //通过左腕坐标减去左手肘坐标获取左侧小臂方向向量，左肩坐标减去左手肘坐标获取左侧大臂方向向量，用Vector3.Angel求这两个向量的夹角
                float rightangle = Vector3.Angle((GetVector3FromJoint(rightWrist)) - (GetVector3FromJoint(rightElbow)), (GetVector3FromJoint(rightShoulder)) - (GetVector3FromJoint(rightElbow)));
                Vector3 vleft= (GetVector3FromJoint(leftElbow)) - (GetVector3FromJoint(leftShoulder));
                //通过左手肘坐标减去左肩坐标获取左侧大臂方向向量
                Vector3 vright = (GetVector3FromJoint(rightElbow)) - (GetVector3FromJoint(rightShoulder));
                leftLastAngle = leftangle;
                rightLastAngle = rightangle;
                SliderValue.text = (Mathf.Floor(smoothFactor.value) - 1).ToString();
                DistanceText.text = DistanceSlider.value.ToString();
                if (leftAngleList.Count >= smoothFactor.value) 
                    leftAngleList.RemoveAt(0);//在平滑列表大于设定的平滑窗口时，剔除列表中最早一帧数据
                leftAngleList.Add(leftLastAngle);
                leftangle = 0;
                foreach (float item in leftAngleList)
                {
                    leftangle += item;
                }
                leftangle /= leftAngleList.Count; //计算整个平滑窗口内的平均数

                if (rightAngleList.Count >= smoothFactor.value)
                    rightAngleList.RemoveAt(0);

                rightAngleList.Add(rightLastAngle);

                rightangle = 0;
                foreach (float item in rightAngleList)
                {
                    rightangle += item;
                }
                rightangle /= rightAngleList.Count;

                if (leftvlist.Count >= smoothFactor.value)
                    leftvlist.RemoveAt(0);
            
                    leftvlist.Add(vleft);
                
                vleft = Vector3.zero;
                foreach (Vector3 item in leftvlist)
                {
                    vleft += item; 
                }
                vleft /= leftvlist.Count;


                if (rightvlist.Count >= smoothFactor.value)
                    rightvlist.RemoveAt(0);
                rightvlist.Add(vright);
                vright = Vector3.zero;
                foreach (Vector3 item in rightvlist)
                {
                    vright +=item ;
                }
                vright /= rightvlist.Count;

                Vector3 vleftrad = getSpherical(vleft.x, vleft.y, vleft.z);
                Vector3 vrightrad = getSpherical(vright.x, vright.y, vright.z);

                lumislider1.value = (float)Scale(leftangle, 45, 180, 0, 1); //修改亮度指示器数据
                lumislider2.value = (float)Scale(rightangle, 45, 180, 0, 1) ;
                lightl.intensity = (float)Scale(leftangle, 45, 180, 0, 100); 
                lightr.intensity = (float)Scale(rightangle, 45, 180, 0, 100);
                var beaml = lightl.GetComponent<VLB.VolumetricLightBeam>();
                beaml.alphaOutside = lumislider1.value; 
                var beamr = lightl.GetComponent<VLB.VolumetricLightBeam>();
                beamr.alphaOutside = lumislider2.value;
                


                Vector3 vleftroat = new Vector3(vleft.x, vleft.y, -vleft.z);
                Vector3 vrightroat = new Vector3(vright.x, vright.y, -vright.z);
                lightl.transform.LookAt(lightl.transform.position + vleftroat);
                lightr.transform.LookAt(lightr.transform.position +vrightroat);

                FillSliderByCenter(sliderx1, vleftrad.x);
                FillSliderByCenter(slidery1, vleftrad.y);    
                FillSliderByCenter(sliderx2, vrightrad.x);
                FillSliderByCenter(slidery2, vrightrad.y);
                

                LLumi.text = Mathf.Ceil((float)(Scale(leftangle, 45, 180, 0, 100))).ToString();
                RLumi.text = Mathf.Ceil((float)(Scale(rightangle, 45, 180, 0, 100))).ToString();
                LOrit.text = Mathf.Ceil(vleftrad.x).ToString()+","+ Mathf.Ceil(vleftrad.y).ToString();
                ROrit.text = Mathf.Ceil(vrightrad.x).ToString()+","+Mathf.Ceil(vrightrad.y).ToString();

                int ch1 = Mathf.CeilToInt((float)(Scale(leftangle, 35, 180, 0, 255)));
                int ch2 = Mathf.CeilToInt((float)(Scale(vleftrad.x, -90, 90, 0, 255)));
                int ch3 = Mathf.CeilToInt((float)(Scale(vleftrad.y, -90, 90, 0, 255)));
                int ch4 = Mathf.CeilToInt((float)(Scale(rightangle, -35, 180, 0, 255)));
                int ch5 = Mathf.CeilToInt((float)(Scale(vrightrad.x, -90, 90, 0, 255)));
                int ch6 = Mathf.CeilToInt((float)(Scale(vrightrad.y, -90, 90, 0, 255)));



                Ch1Num.text = ch1.ToString();
                Ch2Num.text = ch2.ToString();
                Ch3Num.text = ch3.ToString();
                Ch4Num.text = ch4.ToString();
                Ch5Num.text = ch5.ToString();
                Ch6Num.text = ch6.ToString();

                Ch1Bar.value = ch1;
                Ch2Bar.value = ch2;
                Ch3Bar.value = ch3;
                Ch4Bar.value = ch4;
                Ch5Bar.value = ch5;
                Ch6Bar.value = ch6;

                DMXText2.text = "00000000000\n0000000000011\n0111111111111\n0" + GetBin(ch1) +
                    "1111\n0" + GetBin(ch2) +
                    "1111\n0" + GetBin(ch3) +
                    "1111\n0" + GetBin(ch4) +
                    "1111\n0" + GetBin(ch5) +
                    "1111\n0" + GetBin(ch6) + "1111";
             string DMXText16Unparsed = "00000000000000000000001101111111111110" + GetBin(ch1) +
                    "11110" + GetBin(ch2) +
                    "11110" + GetBin(ch3) +
                    "11110" + GetBin(ch4) +
                    "11110" + GetBin(ch5) +
                    "11110" + GetBin(ch6) + "111111111";

                
            List<byte> result = new List<byte>();
            for (int i = 0; i < DMXText16Unparsed.Length; i += 8)
            {
                result.Add(Convert.ToByte(DMXText16Unparsed.Substring(i, 8), 2));
                //以8个二进制字符为一组生成子字符串List
            }
            List<string> result16 = new List<string>();
            DMXText16.text = BitConverter.ToString(result.ToArray()).Replace("-", " ");
            //将上述List转化为数组，按条转化为16进制并添加空格

                Serial.WriteHex(result.ToArray(), 0, result.Count);

                RefreshBodyObject(body, _Bodies[body.TrackingId]);

            }
        }

    }
    private double Scale(float value,float min, float max, float minScale, float maxScale)
    {
        float scaled = 0 ;
        scaled = minScale +(value - min) / (max - min) * (maxScale - minScale); 
        return scaled;
    }

    /// <summary>
    /// 双向填充滑条 
    /// </summary>
    /// <param name="_slider">要双向填充的滑条</param>
    /// <param name="_value">填充的数值</param>
    public void FillSliderByCenter(Slider _slider, float _value) //
    {
        if (_value > 0)
        {
            _slider.value = _value;
            _slider.fillRect.anchorMin = new Vector2(0.5f, 0); 
            _slider.fillRect.anchorMax = new Vector2(_slider.handleRect.anchorMin.x, 1);
        }
        else
        {
            _slider.value = _value;
            _slider.fillRect.anchorMin = new Vector2(_slider.handleRect.anchorMin.x, 0);
            _slider.fillRect.anchorMax = new Vector2(0.5f, 1);
        }
    }

    public string GetBin (int _inInt)
    {
        string str = Convert.ToString(_inInt, 2); //将输入转化为二进制
        int k = str.Length;
        for (int i = 0; i < 8 -k; i++) //不足8位时在最前面补0以符合Frame格式
        {
            str = "0" + str;
        }
        return str;
    } 
    public Vector3 getSpherical(float x, float y, float z)
    {
        return new Vector3((Mathf.Atan(x/Mathf.Abs(z)))*Mathf.Rad2Deg,(Mathf.Atan(y/Mathf.Sqrt(x*x+z*z)))* Mathf.Rad2Deg, Mathf.Sqrt(x*x+y*y+z*z));
    }
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }

            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if (targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return Color.green;

            case Kinect.TrackingState.Inferred:
                return Color.red;

            default:
                return Color.black;
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}


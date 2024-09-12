using pxCore;
using pxCore.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace pxKestrelLibrary
{
    [Browsable(true)]
    [Category("_Kestrel")]
    [DisplayName("xVLV103")]
    [Description("Basic On-Off Valve")]
    [Serializable()]
    public class xVLV103 : EvaluatableFB
    {
        
        string mOpenCmdTagName = "";
        string mCloseCmdTagName = "";

        bool mOpenCmdType = true;
        [CustomAttributes.SerializableProperty(nameof(OpenCmdType))]
        [Category("_Object")]
        public bool OpenCmdType { get => mOpenCmdType ; set { mOpenCmdType = value; _populateCommTagsList(); } }


        string mValveName = "";
        [CustomAttributes.SerializableProperty(nameof(ValveName))]
        [Category("_Object")]
        public string ValveName { get => mValveName; set { mValveName = value; _populateCommTagsList(); } }

        string mPLC = "";
        [CustomAttributes.SerializableProperty(nameof(PLC))]
        [Category("_Object")]
        public string PLC { get => mPLC; set { mPLC = value; _populateCommTagsList(); } }


        private BaseValve theValve = new BaseValve();

        // Motor pins
        private Pin? mPinOpen, mPinClosed;

        List<string> mErrors = new List<string>();

        public xVLV103(string ObjectName) : base(ObjectName)
        {
            BlockColor = LibraryResources.BlockColor;
        }
        public override void ModelStatusChanged(object sender, bool value)
        {
            InputChanged = true;
        }

        public override void ModelUpdateTimeChanged(IModel sender, float value)
        {

        }
        public override void ParentChanged(IBaseModel newParent)
        {
            InputChanged = true;
        }

        public override void AddPins()
        {
            // ****
            // inputs
            //mPinOpenCmd = AddInputPin("OPN", eDataType.BOOL, false, true, true, false, -1, "Open Command");
            //mPinOpenCmd.AllowToChangeDataType = false;
            //mPinLoad = AddInputPin("LOAD", eDataType.FLOAT32, 50.0d, true, true, true, -1, "Valve load (%)");
            //mPinLoad.Visible = false;

            // *******
            // outputs
            mPinClosed = AddOutputPin("CloseFb", eDataType.BOOL, false, true, true, true, -1, "Closed Status");
            mPinClosed.AllowToChangeDataType = false;
            //mPinOpening = AddOutputPin("OPG", eDataType.BOOL, false, true, true, true, -1, "Opening Status");
            //mPinOpening.AllowToChangeDataType = false;
            mPinOpen = AddOutputPin("OpenFb", eDataType.BOOL, false, true, true, true, -1, "Open Status");
            mPinOpen.AllowToChangeDataType = false;
            //mPinClosing = AddOutputPin("CLG", eDataType.BOOL, false, true, true, true, -1, "Closing Status");
            //mPinClosing.AllowToChangeDataType = false;

            AddPropertyPins();
        }


        public override void AddParams()
        {
            Parameter p;

            // p = New Parameter(Me,"OpenTime", 5.0, GetType(Single))
            // p.Description = "Valve open time (seconds) @0% load"
            // AddHandler p.ValueChanged, AddressOf ParameterChanged
            // Params.Add(p)

            // p = New Parameter(Me,"CloseTime", 2.0, GetType(Single))
            // p.Description = "Valve close time (seconds) @0% load"
            // AddHandler p.ValueChanged, AddressOf ParameterChanged
            // Params.Add(p)

        }

        public override void ParameterChanged(object sender, object value)
        {
            // Select Case sender.Name
            // Case "OpenTime"
            // theValve.OpenTime = value
            // Case "CloseTime"
            // theValve.CloseTime = value
            // End Select
            // RaiseParamChanged(sender)
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Valve open time (milliseconds) @0% load")]
        [CustomAttributes.SerializableProperty(nameof(OpenTime))]
        [CustomAttributes.PinableProperty(true)]
        public float OpenTime
        {
            get
            {
                return theValve.OpenTime;
            }
            set
            {
                theValve.OpenTime = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Valve close time (milliseconds) @0% load")]
        [CustomAttributes.SerializableProperty(nameof(CloseTime))]
        [CustomAttributes.PinableProperty(true)]
        public float CloseTime
        {
            get
            {
                return theValve.CloseTime;
            }
            set
            {
                theValve.CloseTime = value;
                OnPropertyChanged();
            }
        }

        public override void Evaluate(bool forceUpdate)
        {

            bool mValveOpened = theValve.Status == BaseValve.eValveStatus.Open | theValve.Status == BaseValve.eValveStatus.Opening;
            bool mValveClosed = theValve.Status == BaseValve.eValveStatus.Closing | theValve.Status == BaseValve.eValveStatus.Closed;

            //theValve.ValveLoad = mPinLoad.ToDouble();

            mErrors.Clear();

            //read PLC command
            if (mOpenCmdType)
            {
                var _cmd = MathFunctions.ToBoolean(_getTag(mOpenCmdTagName)?.Value);
                if (_cmd)
                    theValve.Open();
                else
                    theValve.Close();
            }
            else
            {
                var _cmd = MathFunctions.ToBoolean(_getTag(mCloseCmdTagName)?.Value);
                if (_cmd)
                    theValve.Close();
                else
                    theValve.Open();
            }

            theValve.Evaluate();

            mPinOpen.Value = theValve.Status == BaseValve.eValveStatus.Open;
            //mPinOpening.Value = theValve.Status == BaseValve.eValveStatus.Opening;
            mPinClosed.Value = theValve.Status == BaseValve.eValveStatus.Closed;
            //mPinClosing.Value = theValve.Status == BaseValve.eValveStatus.Closing;

            IndicationChanged = true;


            if (mErrors.Count > 0)
            {
                StatusMsg = "Tag Errors:\n" + string.Join('\n', mErrors);
                StatusOk = false;
            }
            else
            {
                StatusMsg = "Ok";
                StatusOk = true;
            }
        }
        
        private void _populateCommTagsList()
        {
            CommTags.Clear();
            InputCommTagNames.Clear();
            OutputCommTagNames.Clear();

            mOpenCmdTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + ValveName + "_fOutOpen";
            mCloseCmdTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + ValveName + "_fOutClos";

            if(mOpenCmdType) 
                InputCommTagNames.Add(mOpenCmdTagName);
            else
                InputCommTagNames.Add(mCloseCmdTagName);
        }

        private RuntimeTag? _getTag(string name)
        {
            var _tag = CommTags.ContainsKey(name) ? CommTags[name] : null;

            if (_tag is not null && _tag.Failed)
                mErrors.Add(name);

            return _tag;

        }

    }
}
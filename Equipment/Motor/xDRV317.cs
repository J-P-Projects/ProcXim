using pxCore;
using pxCore.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static pxCore.GlobalFunctions;

namespace pxKestrelLibrary
{

    [Browsable(true)]
    [Category("_Kestrel")]
    [DisplayName("xDRV317")]
    [Description("Basic Motor")]
    [Serializable()]
    public class xDRV317 : EvaluatableFB
    {

        #region properties

        public enum eSpeedType
        {
            Fixed,
            Variable
        }

        string mMotorName = "";
        [CustomAttributes.SerializableProperty(nameof(MotorName))]
        [Category("_Object")]
        public string MotorName { get => mMotorName; set { mMotorName = value; _populateCommTagsList(); } }

        string mPLC = "";
        [CustomAttributes.SerializableProperty(nameof(PLC))]
        [Category("_Object")]
        public string PLC { get => mPLC; set { mPLC = value; _populateCommTagsList(); } }


        string mOutFwdTagSuffix = "_fOutFwd";
        [CustomAttributes.SerializableProperty(nameof(OutFwdTagSuffix))]
        [Category("_Tags")]
        public string OutFwdTagSuffix { get => mOutFwdTagSuffix ; set { mOutFwdTagSuffix = value; _populateCommTagsList(); } }

        string mOutRevTagSuffix  = "_fOutRev";
        [CustomAttributes.SerializableProperty(nameof(OutRevTagSuffix))]
        [Category("_Tags")]
        public string OutRevTagSuffix { get => mOutRevTagSuffix; set { mOutRevTagSuffix = value; _populateCommTagsList(); } }

        string mRunningFwdTagSuffix { get; set; } = "f_fDigIn";
        [CustomAttributes.SerializableProperty(nameof(RunningFwdTagSuffix))]
        [Category("_Tags")]
        public string RunningFwdTagSuffix { get => mRunningFwdTagSuffix; set { mRunningFwdTagSuffix = value; _populateCommTagsList(); } }

        string mRunningRevTagSuffix = "g_fDigIn";
        [CustomAttributes.SerializableProperty(nameof(RunningRevTagSuffix))]
        [Category("_Tags")]
        public string RunningRevTagSuffix { get => mRunningRevTagSuffix; set {mRunningRevTagSuffix = value; _populateCommTagsList(); } }

        string mFieldStartFwdTagSuffix  = "_cStFldF";
        [CustomAttributes.SerializableProperty(nameof(FieldStartFwdTagSuffix))]
        [Category("_Tags")]
        public string FieldStartFwdTagSuffix { get => mFieldStartFwdTagSuffix;  set { mFieldStartFwdTagSuffix = value; _populateCommTagsList(); } }

        string mFieldStartRevTagSuffix  = "_cStFldR";
        [CustomAttributes.SerializableProperty(nameof(FieldStartRevTagSuffix))]
        [Category("_Tags")]
        public string FieldStartRevTagSuffix { get => mFieldStartRevTagSuffix; set { mFieldStartRevTagSuffix = value; _populateCommTagsList(); } }

        string mFieldStopTagSuffix  = "k_fDigIn";
        [CustomAttributes.SerializableProperty(nameof(FieldStopTagSuffix))]
        [Category("_Tags")]
        public string FieldStopTagSuffix { get => mFieldStopTagSuffix; set{ mFieldStopTagSuffix = value; _populateCommTagsList(); } }


        string mOLTripTagSuffix = "h_fDigIn";
        [CustomAttributes.SerializableProperty(nameof(OLTripTagSuffix))]
        [Category("_Tags")]
        public string OLTripTagSuffix { get => mOLTripTagSuffix; set {mOLTripTagSuffix = value; _populateCommTagsList(); } }

        string mELTripTagSuffix = "j_fDigIn";
        [CustomAttributes.SerializableProperty(nameof(ELTripTagSuffix))]
        [Category("_Tags")]
        public string ELTripTagSuffix { get => mELTripTagSuffix; set {mELTripTagSuffix = value; _populateCommTagsList(); } }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Motor fixed speed or speed at 100% reference (RPM)")]
        [CustomAttributes.SerializableProperty(nameof(MaxRPM))]
        [CustomAttributes.PinableProperty(true)]
        public int MaxRPM
        {
            get
            {
                return theMotor.MaxRPM;
            }
            set
            {
                theMotor.MaxRPM = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Full Load Motor Current (A) at 100% load")]
        [CustomAttributes.SerializableProperty(nameof(FLC))]
        [CustomAttributes.PinableProperty(true)]
        public float FLC
        {
            get
            {
                return theMotor.FLC;
            }
            set
            {
                theMotor.FLC = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Current @0% load (in % of FLC)")]
        [CustomAttributes.SerializableProperty(nameof(IddleCurrent))]
        [CustomAttributes.PinableProperty(true)]
        public float IddleCurrent
        {
            get
            {
                return theMotor.IddleCurrent;
            }
            set
            {
                theMotor.IddleCurrent = value;
                OnPropertyChanged();
            }
        }
        [Browsable(true)]
        [Category("_Object")]
        [Description("Current inrush time (in milliseconds)")]
        [CustomAttributes.SerializableProperty(nameof(InRushTime))]
        [CustomAttributes.PinableProperty(true)]
        public float InRushTime
        {
            get
            {
                return theMotor.InRushTime;
            }
            set
            {
                theMotor.InRushTime = value;
                OnPropertyChanged();
            }
        }
        [Browsable(true)]
        [Category("_Object")]
        [Description("Current inrush peak factor (in % of FLC)")]
        [CustomAttributes.SerializableProperty(nameof(InRushFactor))]
        [CustomAttributes.PinableProperty(true)]
        public float InRushFactor
        {
            get
            {
                return theMotor.InRushPeak;
            }
            set
            {
                theMotor.InRushPeak = value;
                OnPropertyChanged();
            }
        }
        private bool mIsReversible;
        [Browsable(true)]
        [Category("_Object")]
        [Description("motor is reversible or forward only")]
        [CustomAttributes.SerializableProperty(nameof(IsReversible))]
        [CustomAttributes.PinableProperty(true)]
        public bool IsReversible
        {
            get
            {
                return mIsReversible;
            }
            set
            {
                if (mIsReversible == value)
                    return;
                //ChangeIsReversible(value);
                mIsReversible = value;
                _populateCommTagsList();
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Motor inherent acceleration time (milliseconds) @0% load")]
        [CustomAttributes.SerializableProperty(nameof(AccelTime))]
        [CustomAttributes.PinableProperty(true)]
        public float AccelTime
        {
            get
            {
                return theMotor.AccelTime;
            }
            set
            {
                theMotor.AccelTime = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Motor inherent decceleration time (milliseconds) @0% load")]
        [CustomAttributes.SerializableProperty(nameof(DeccelTime))]
        [CustomAttributes.PinableProperty(true)]
        public float DeccelTime
        {
            get
            {
                return theMotor.DeccelTime;
            }
            set
            {
                theMotor.DeccelTime = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [Description("Fixed (DOL) or variable speed (VSD) motor drive type")]
        [CustomAttributes.SerializableProperty(nameof(SpeedType))]
        //[CustomAttributes.PinableProperty(true)]
        public eSpeedType SpeedType
        {
            get
            {
                return mSpeedType;
            }
            set
            {
                //ChangeSpeedType(value);
                mSpeedType = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region fields
        private BaseMotor theMotor = new BaseMotor();

        private eSpeedType mSpeedType = eSpeedType.Fixed;
        private bool mRunningDirectionIsFwd;

        // Motor pins
        private Pin? mPinLoad, mOLTripE, mStFldF, mSpFld, mELTripE, mCFSClsE, mFieldReset, mStFldR, mRngF, mRngR, mSPV, mSpeedRef;  // mPinStartFwd, mPinStartRev, mPinSpeedIn, mPinLoad, mPinRunningFwd, mPinRunningRev, mPinSpeedOut, mPinCurrent;
        
        //private fields
        bool mStartForwardCommand, mStartReverseCommand, mRunningForward, mRunningReverse;
        double mSpeed;

        string mStartFwdTagName, mSrtartRevTagName, mRunningFwdTagName, mRunningRevTagName, mFieldStartFwdTagName, mFieldStartRevTagName, mFieldStopTagName, mOLTripTagName, mELTripTagName;

        List<string> mErrors = new List<string>();

        #endregion

        public xDRV317(string ObjectName) : base(ObjectName)
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
            mPinLoad = AddInputPin("Load", eDataType.FLOAT32, 50.0d, true, true, true, -1, "Motor load (%)");
            mOLTripE = AddInputPin("fOLTripE", eDataType.BOOL, false, true, true, true);
            mStFldF = AddInputPin("cStFldF",eDataType.BOOL, false, true, true);
            mSpFld = AddInputPin("cSpFld",eDataType.BOOL, false, true, true);
            mELTripE = AddInputPin("fELTripE", eDataType.BOOL, false, true, true);
            mCFSClsE = AddInputPin("fCFSClsE", eDataType.BOOL, false, true, true);
            mFieldReset = AddInputPin("cFieldReset",eDataType.BOOL, false, true, true);
            mStFldR = AddInputPin("cStFldR", eDataType.BOOL, false, true, true);
            mSpeedRef = AddInputPin("speedRef", eDataType.FLOAT32, 0.0, true, true);

            // *******
            // outputs
            mRngF = AddOutputPin("sRngF",eDataType.BOOL,false, true, true);
            mRngR = AddOutputPin("sRngR", eDataType.BOOL, false, true, true);
            mSPV = AddOutputPin("sSPV", eDataType.FLOAT32, 0.0, true, true);

            AddPropertyPins();

        }

        public override void AddParams()
        {
            //do nothing here
        }

        public override void ParameterChanged(object sender, object value)
        {
            //do nothing here
        }
        
        private void ChangeIsReversible(bool NewValue)
        {
            //if (NewValue)
            //{
            //    // show new 'Reversible' pins
            //    mPinStartRev.Visible = true;
            //    mPinRunningRev.Visible = true;
            //    mIsReversible = true;
            //}
            //else if (!mPinStartRev.CanConnect | !mPinRunningRev.CanConnect) // if changing from Reversible to non-reversible, cehck if RunReverse or RngReverse pins are conncted
            //{
            //    //_MsgBox("'Reversible' property can't be changed because Start Reverse or Running Reverse pins are connected"); // ((int)MsgBoxStyle.Exclamation).ToString(), (MessageBoxButtons)Convert.ToInt32("Error"));
            //    mIsReversible = true;
            //}
            //else
            //{
            //    // hide not required pins
            //    mPinStartRev.Visible = false;
            //    mPinRunningRev.Visible = false;
            //    mPinStartRev.Value = false;
            //    mIsReversible = false;
            //}

        }

        private void ChangeSpeedType(eSpeedType NewValue)
        {
            //if (NewValue == eSpeedType.Variable)
            //{
            //    // show new 'Reversible' pins
            //    mPinSpeedIn.Visible = true;
            //    mSpeedType = eSpeedType.Variable;
            //}
            //else if (!mPinSpeedIn.CanConnect) // if changing from fixed to variable, check if PinSpeed pin are conncted
            //{
            //    //_MsgBox("'SpeedType' property can't be changed because SpeedInput is connected"); //, ((int)MsgBoxStyle.Exclamation).ToString());
            //    mSpeedType = eSpeedType.Variable;
            //}
            //else
            //{
            //    // hide not required pins
            //    mPinSpeedIn.Visible = false;
            //    mSpeedType = eSpeedType.Fixed;
            //}

        }

        public override void Evaluate(bool forceUpdate)
        {

            mErrors.Clear();

            //get data from comms
            _readCommsTags();

            //// check if there is a start command and the motor is not running
            if (mStFldF.ToBoolean() | mStartForwardCommand) //& (theMotor.Status == BaseMotor.eMotorStatus.Running | theMotor.Status == BaseMotor.eMotorStatus.Accelerating))
            {
                //theMotor.Stop();
                mRunningForward = true;
            }
            else
            {
                mRunningForward = false;

                if (mStFldR.ToBoolean() | mStartReverseCommand)
                {
                    //theMotor.Stop();
                    mRunningReverse = true;
                }
                else
                {
                    mRunningReverse = false;
                }
            }

            mRngF.Value = mRunningForward;
            mRngR.Value = mRunningReverse;

            //if (mSpeedType == eSpeedType.Fixed)
            //{
            //    theMotor.SpeedRef = 100;
            //}
            //else
            //{
            //    theMotor.SpeedRef = Convert.ToSingle(mPinSpeedIn.Value);
            //}

            //theMotor.MotorLoad = Convert.ToSingle(mPinLoad.Value);

            //// check if there is s start command and the motor is not running
            //if( (mPinStartFwd.ToBoolean() | mPinStartRev.ToBoolean()) & !(theMotor.Status == BaseMotor.eMotorStatus.Running | theMotor.Status == BaseMotor.eMotorStatus.Accelerating))
            //{
            //    theMotor.Start();
            //}

            //theMotor.Evaluate();

            //bool mMotorStopped = theMotor.Status == BaseMotor.eMotorStatus.Deccelerating | theMotor.Status == BaseMotor.eMotorStatus.Stopped;
            //bool mMotorRunning = (theMotor.Status == BaseMotor.eMotorStatus.Running | theMotor.Status == BaseMotor.eMotorStatus.Accelerating) & !mMotorStopped;

            //mRunningDirectionIsFwd =mPinStartFwd.ToBoolean() & mMotorRunning;

            //mPinRunningFwd.Value = mPinStartFwd.ToBoolean() & mMotorRunning & mRunningDirectionIsFwd;
            //mPinRunningRev.Value = mPinStartRev.ToBoolean() & mMotorRunning & !mRunningDirectionIsFwd;

            if (SpeedType == eSpeedType.Fixed)
                mSPV.Value = (mRunningForward | mRunningReverse) ? MaxRPM : 0.0; //theMotor.SpeedOut;
            else
                mSPV.Value = (mRunningForward | mRunningReverse) ? MathFunctions.ScaleValue(mSpeedRef.ToDouble(), 0, 100, 0, MaxRPM) : 0.0;
            //mPinCurrent.Value = theMotor.Current;

            //write comms tags
            _writeCommsTags();


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

            IndicationChanged = true;

        }

        private void _readCommsTags()
        {
            mStartForwardCommand = MathFunctions.ToBoolean(_getTag(mStartFwdTagName)?.Value);
            mStartReverseCommand = MathFunctions.ToBoolean(_getTag(mSrtartRevTagName)?.Value);           
        }

        private void _writeCommsTags()
        {
            _setTag(mRunningFwdTagName, mRunningForward);
            _setTag(mRunningRevTagName, mRunningReverse);
            _setTag(mFieldStartFwdTagName, mStFldF?.Value);
            _setTag(mFieldStartRevTagName, mStFldR?.Value);
            _setTag(mFieldStopTagName, !mSpFld?.ToBoolean());
            _setTag(mOLTripTagName, !mOLTripE?.ToBoolean());
            _setTag(mELTripTagName, !mELTripE?.ToBoolean());
        }

        private void _populateCommTagsList()
        {
            
            CommTags.Clear();
            InputCommTagNames.Clear();
            OutputCommTagNames.Clear();

            mStartFwdTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + OutFwdTagSuffix;
            mSrtartRevTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + OutRevTagSuffix;

            mRunningFwdTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + RunningFwdTagSuffix;
            mRunningRevTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + RunningRevTagSuffix;

            //values from pins
            mFieldStartFwdTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + FieldStartFwdTagSuffix;
            mFieldStartRevTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + FieldStartRevTagSuffix;

            mFieldStopTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + FieldStopTagSuffix;
            mOLTripTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + OLTripTagSuffix;
            mELTripTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + MotorName + ELTripTagSuffix;

            //tags to read from the PLC
            if (!string.IsNullOrEmpty(OutFwdTagSuffix)) InputCommTagNames.Add(mStartFwdTagName);
            if (!string.IsNullOrEmpty(OutRevTagSuffix) && IsReversible) InputCommTagNames.Add(mSrtartRevTagName);

            //tags to write to the PLC
            if (!string.IsNullOrEmpty(RunningFwdTagSuffix)) OutputCommTagNames.Add(mRunningFwdTagName);
            if (!string.IsNullOrEmpty(RunningRevTagSuffix) && IsReversible) OutputCommTagNames.Add(mRunningRevTagName);
            if (!string.IsNullOrEmpty(FieldStartFwdTagSuffix)) OutputCommTagNames.Add(mFieldStartFwdTagName);
            if (!string.IsNullOrEmpty(FieldStartRevTagSuffix) && IsReversible) OutputCommTagNames.Add(mFieldStartRevTagName);
            if (!string.IsNullOrEmpty(FieldStopTagSuffix)) OutputCommTagNames.Add(mFieldStopTagName);
            if (!string.IsNullOrEmpty(OLTripTagSuffix)) OutputCommTagNames.Add(mOLTripTagName);
            if (!string.IsNullOrEmpty(ELTripTagSuffix)) OutputCommTagNames.Add(mELTripTagName);
        }

        private void _setTag(string name, object? value)
        {
            if (value is null) return;

            var _tag = CommTags.ContainsKey(name) ? CommTags[name] : null;

            if (_tag is not null)
            {
                if (_tag.Failed) mErrors.Add(name);
                _tag.Value = value;
            }
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
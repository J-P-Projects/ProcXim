using pxCore;
using pxCore.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace pxKestrelLibrary
{

    [Browsable(true)]
    [Category("_Kestrel")]
    [DisplayName("xANL211")]
    [Description("Analog Output")]
    [Serializable()]
    public class xANL211 : EvaluatableFB
    {
        private BaseAnalogIn theAnalogOut = new BaseAnalogIn();

        private float mRawValue;

        // pins
        //private InputPin? mPinInput;
        private OutputPin? mPinOutput;
        private float mModelUpdateTime;

        List<string> mErrors = new List<string>();
        string mAnalogTagName = "";

        //properties
        string mOutputName = "";
        [CustomAttributes.SerializableProperty(nameof(OutputName))]
        [Category("_Object")]
        public string OutputName { get => mOutputName; set { mOutputName = value; _populateCommTagsList(); } }

        string mPLC = "";
        [CustomAttributes.SerializableProperty(nameof(PLC))]
        [Category("_Object")]
        public string PLC { get => mPLC; set { mPLC = value; _populateCommTagsList(); } }

       
        public string mTagSuffix = "_rAnOutSc";
        [CustomAttributes.SerializableProperty(nameof(TagSuffix))]
        [Category("_Tags")]
        public string TagSuffix { get => mTagSuffix; set { mTagSuffix = value; _populateCommTagsList(); } }

        [CustomAttributes.SerializableProperty(nameof(EngZero))]
        [Category("_Object")]
        public float EngZero { get; set; }

        [CustomAttributes.SerializableProperty(nameof(EngFull))]
        [Category("_Object")]
        public float EngFull { get; set; }


        public xANL211(string ObjectName) : base(ObjectName)
        {
            BlockColor = LibraryResources.BlockColor;
        }


        public override void ModelUpdateTimeChanged(IModel sender, float value)
        {
            mModelUpdateTime = value;
            SetFilterScanTime();
        }

        public override void ModelStatusChanged(object sender, bool value)
        {
            InputChanged = true;
        }

        public override void ParentChanged(IBaseModel newParent)
        {
            if (newParent is null || !(newParent is IModel))
                return;
            IModel m = (IModel)newParent;
            mModelUpdateTime = m.CycleTime;
            SetFilterScanTime();
            // AddHandler t.UpdateTimeChanged, AddressOf ModelTimeUpdateChanged
        }
        public override void AddPins()
        {
            // outputs
            mPinOutput = AddOutputPin("EngVal", eDataType.FLOAT32, 0.0d, true, true, true, -1, "Output value");

            AddPropertyPins();

        }


        public override void AddParams()
        {
            //do nothing here
        }

        [Browsable(true)]
        [Category("_Object")]
        [CustomAttributes.SerializableProperty(nameof(RawZero))]
        [CustomAttributes.PinableProperty(true)]
        public float RawZero
        {
            get
            {
                return theAnalogOut.LowRange;
            }
            set
            {
                theAnalogOut.LowRange = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [CustomAttributes.SerializableProperty(nameof(RawFull))]
        [CustomAttributes.PinableProperty(true)]
        public float RawFull
        {
            get
            {
                return theAnalogOut.HighRange;
            }
            set
            {
                theAnalogOut.HighRange = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [CustomAttributes.SerializableProperty(nameof(FilterTime))]
        [CustomAttributes.PinableProperty(true)]
        public float FilterTime
        {
            get
            {
                return theAnalogOut.FilterTime;
            }
            set
            {
                theAnalogOut.FilterTime = value;
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [Category("_Object")]
        [CustomAttributes.SerializableProperty(nameof(RandomRatio))]
        [CustomAttributes.PinableProperty(true)]
        public float RandomRatio
        {
            get
            {
                return theAnalogOut.RandomRatio;
            }
            set
            {
                theAnalogOut.RandomRatio = value;
                OnPropertyChanged();
            }
        }

        public override void ParameterChanged(object sender, object value)
        {
            //nothing to do here
        }

        private void SetFilterScanTime()
        {
            theAnalogOut.ScanTime = mModelUpdateTime;
        }

        public override void Evaluate(bool forceUpdate)
        {
            //
            mErrors.Clear();

            try
            {
                mRawValue = Convert.ToSingle(_getTag(mAnalogTagName)?.Value);
                theAnalogOut.Enabled = Enabled;
                theAnalogOut.In = (float)MathFunctions.ScaleValue(mRawValue,RawZero,RawFull, EngZero, EngFull);
                mPinOutput.Value = theAnalogOut.Out;
                IndicationChanged = true;
            }
            catch { }

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

       

        private RuntimeTag? _getTag(string name)
        {
            var _tag = CommTags.ContainsKey(name) ? CommTags[name] : null;

            if (_tag is not null && _tag.Failed)
                mErrors.Add(name);

            return _tag;

        }

        private void _populateCommTagsList()
        {
            CommTags.Clear();
            InputCommTagNames.Clear();
            OutputCommTagNames.Clear();

            mAnalogTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + OutputName + TagSuffix;

            InputCommTagNames.Add(mAnalogTagName);

        }


    }
}
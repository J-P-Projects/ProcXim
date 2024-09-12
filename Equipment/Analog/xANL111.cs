using pxCore;
using pxCore.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace pxKestrelLibrary
{

    [Browsable(true)]
    [Category("_Kestrel")]
    [DisplayName("xANL111")]
    [Description("Analog Input")]
    [Serializable()]
    public class xANL111 : EvaluatableFB
    {
        private BaseAnalogIn theAnalogIn = new BaseAnalogIn();

        // pins
        private InputPin? mPinInput;
        private OutputPin? mPinOutput;
        private float mModelUpdateTime;

        List<string> mErrors = new List<string>();
        string mAnalogTagName = "";

        //properties
        string mInputName = "";
        [CustomAttributes.SerializableProperty(nameof(InputName))]
        [Category("_Object")]
        public string InputName { get => mInputName ; set { mInputName = value; _populateCommTagsList(); } }

        string mPLC = "";
        [CustomAttributes.SerializableProperty(nameof(PLC))]
        [Category("_Object")]
        public string PLC { get => mPLC; set { mPLC = value; _populateCommTagsList(); } }

        public string mTagSuffix = "_fAnaIn";
        [CustomAttributes.SerializableProperty(nameof(TagSuffix))]
        [Category("_Tags")]
        public string TagSuffix { get => mTagSuffix; set { mTagSuffix = value; _populateCommTagsList(); } } 

        [CustomAttributes.SerializableProperty(nameof(EngZero))]
        [Category("_Object")]
        public float EngZero { get; set; }

        [CustomAttributes.SerializableProperty(nameof(EngFull))]
        [Category("_Object")]
        public float EngFull { get; set; }


        public xANL111(string ObjectName) : base(ObjectName)
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
            // ****
            // inputs
            // mPinEnable = AddBlockPin("Enb", enumIOType.Input, eDataType.BOOL, True, True, True,,, "Enable / Gate command")

            mPinInput = AddInputPin("EngVal", eDataType.FLOAT32, 0.0d, true, true, true, -1, "Input value");

            // *******
            // outputs
            mPinOutput = AddOutputPin("RawVal", eDataType.FLOAT32, 0.0d, true, true, true, -1, "Output value");

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
                return theAnalogIn.LowRange;
            }
            set
            {
                theAnalogIn.LowRange = value;
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
                return theAnalogIn.HighRange;
            }
            set
            {
                theAnalogIn.HighRange = value;
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
                return theAnalogIn.FilterTime;
            }
            set
            {
                theAnalogIn.FilterTime = value;
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
                return theAnalogIn.RandomRatio;
            }
            set
            {
                theAnalogIn.RandomRatio = value;
                OnPropertyChanged();
            }
        }

        public override void ParameterChanged(object sender, object value)
        {
            // do nothing here
        }

        public override void Evaluate(bool forceUpdate)
        {

            mErrors.Clear();

            var _input = Math.Clamp(mPinInput.ToDouble(), EngZero, EngFull);


            try
            {
                theAnalogIn.Enabled = Enabled;
                theAnalogIn.In = (float)MathFunctions.ScaleValue(_input, EngZero, EngFull,RawZero,RawFull);
                mPinOutput.Value = theAnalogIn.Out;
                IndicationChanged = true;
                StatusOk = true;
                StatusMsg = "Ok";
            }
            catch (Exception ex)
            {
                StatusOk = false;
                StatusMsg = ex.Message;
                ShowInfoMessage(eLogInfoType.Error, this, ex.Message);
            }

            //
            _setTag(mAnalogTagName, mPinOutput.Value);

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

        private void SetFilterScanTime()
        {
            theAnalogIn.ScanTime = mModelUpdateTime;
        }
        
        private void _populateCommTagsList()
        {
            CommTags.Clear();
            InputCommTagNames.Clear();
            OutputCommTagNames.Clear();

            mAnalogTagName = PLC + GlobalObjects.EXPRESSION_PART_SEPARATOR + InputName + TagSuffix;

            OutputCommTagNames.Add(mAnalogTagName);

        }

        private void _setTag(string name, object value)
        {
            var _tag = CommTags.ContainsKey(name) ? CommTags[name] : null;

            if (_tag is not null)
            {
                if (_tag.Failed) mErrors.Add(name);
                _tag.Value = value;
            }
        }

    }
}
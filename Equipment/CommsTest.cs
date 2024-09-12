using pxCore;
using pxCore.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;


namespace pxKestrelLibrary
{

    [Browsable(true)]
    [Category("_Kestrel")]
    [DisplayName("_CommsTest")]
    [Description("Block for communication tests")]
    [Serializable()]
    public class CommsTest : EvaluatableFB
    {
        // pins
        private InputPin? mWriteValuePin;
        private OutputPin? mReadValuePin;
        private float mModelUpdateTime;
        List<string> mErrors = new List<string>();


        #region properties

        string mReadTagName = "";
        [CustomAttributes.SerializableProperty(nameof(ReadTagName))]
        [Category("_Tags")]
        public string ReadTagName { get => mReadTagName; set { mReadTagName = value; _populateCommTagsList(); } }

        string mWriteTagName = "";
        [CustomAttributes.SerializableProperty(nameof(WriteTagName))]
        [Category("_Tags")]
        public string WriteTagName { get => mWriteTagName; set { mWriteTagName = value; _populateCommTagsList(); } }

        string mConnectionName = "";
        [CustomAttributes.SerializableProperty(nameof(ConnectionName))]
        [Category("_Object")]
        public string ConnectionName { get => mConnectionName; set { mConnectionName = value; _populateCommTagsList(); } }


        [CustomAttributes.SerializableProperty(nameof(ReadTagDataType))]
        [Category("_Tags")]
        public eDataType ReadTagDataType { get; set; } = eDataType.FLOAT32;

        [CustomAttributes.SerializableProperty(nameof(WriteTagDataType))]
        [Category("_Tags")]
        public eDataType WriteTagDataType { get; set; } = eDataType.FLOAT32;

#endregion

        public CommsTest(string ObjectName) : base(ObjectName)
        {
            BlockColor = Colors.White;
        }

        public override void ModelUpdateTimeChanged(IModel sender, float value)
        {
            mModelUpdateTime = value;
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
            
        }
        public override void AddPins()
        {
            // ****
            // inputs
            // mPinEnable = AddBlockPin("Enb", enumIOType.Input, eDataType.BOOL, True, True, True,,, "Enable / Gate command")

            mWriteValuePin = AddInputPin("ValueToWrite", WriteTagDataType, 0.0d, true, true, true, -1, "Value to write to output tag");

            // *******
            // outputs
            mReadValuePin = AddOutputPin("ReadValue", ReadTagDataType, 0.0d, true, true, true, -1, "Value read from input tag");

            AddPropertyPins();

        }


        public override void AddParams()
        {
            //do nothing here
        }

        public override void ParameterChanged(object sender, object value)
        {
            //do nothig here
        }

        public override void Evaluate(bool forceUpdate)
        {
            //
            mErrors.Clear();

            //
            _readCommsTags();
                        
            //
            _writeCommsTags();

            if (mErrors.Count > 0)
            {
                StatusMsg = "Tag Errors:\n" + string.Join('\n',mErrors);
                StatusOk = false;
            }
            else
            {
                StatusMsg = "Ok";
                StatusOk = true;
            }

        }

        private void _readCommsTags()
        {
            if (string.IsNullOrEmpty(mReadTagName)) return;
            
            var _tagName = ConnectionName + GlobalObjects.EXPRESSION_PART_SEPARATOR + mReadTagName;
           
            var _tag = CommTags.ContainsKey(_tagName) ? CommTags[_tagName] : null;

            if (_tag is not null && !_tag.Failed)
                mReadValuePin.Value = _tag.Value;
            else
                mErrors.Add(_tagName);
            
        }

        private void _writeCommsTags()
        {
            if (string.IsNullOrEmpty(mWriteTagName)) return;

            var _tagName = ConnectionName + GlobalObjects.EXPRESSION_PART_SEPARATOR + mWriteTagName;
            
            var _tag = CommTags.ContainsKey(_tagName) ? CommTags[_tagName] : null;

            if (_tag is not null)
            {
                if (_tag.Failed) mErrors.Add(_tagName);
                _tag.Value = mWriteValuePin.Value;
            }
              
        }

        private void _populateCommTagsList()
        {
            CommTags.Clear();
            InputCommTagNames.Clear();
            OutputCommTagNames.Clear();

            //tags to read from the PLC
            if (!string.IsNullOrEmpty(mReadTagName)) InputCommTagNames.Add(ConnectionName + GlobalObjects.EXPRESSION_PART_SEPARATOR + mReadTagName);

            //write tags to PLC
            if (!string.IsNullOrEmpty(mWriteTagName)) OutputCommTagNames.Add(ConnectionName + GlobalObjects.EXPRESSION_PART_SEPARATOR + mWriteTagName);

        }


    }
}
using System;
using System.Globalization;
using Vector.VLConfig.Data.ConfigurationDataModel;

namespace Vector.VLConfig.CaplGeneration.DataPresenter
{
	internal class DpTriggerConditionCanId : DpTriggerCondition
	{
		private readonly CANIdEvent mEvent;

		public override string ChannelString
		{
			get
			{
				return this.mEvent.ChannelNumber.Value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public override string NameOfCaplEventHandler
		{
			get
			{
				return CaplHelper.MakeCanMsgHandlerString(this.ChannelString);
			}
		}

		public override string TemplateEventHandler
		{
			get
			{
				return "TEventHandler_Condition_Msg";
			}
		}

		public string MsgConditionString
		{
			get
			{
				return CaplHelper.MakeConditionStringCanId(this.mEvent.IdRelation.Value, "this.ID", this.mEvent.LowId.Value, this.mEvent.HighId.Value, this.mEvent.IsExtendedId.Value);
			}
		}

		public DpTriggerConditionCanId(uint triggerIdent, uint conditionIdent, Event conditionEvent) : base(triggerIdent, conditionIdent, conditionEvent)
		{
			this.mEvent = (conditionEvent as CANIdEvent);
		}
	}
}

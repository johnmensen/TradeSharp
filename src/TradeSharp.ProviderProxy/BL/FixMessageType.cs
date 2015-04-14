using System.Collections.Generic;

namespace TradeSharp.ProviderProxy.BL
{
    public enum FixMessageType
    {
        Undefined,              // -
        Heartbeat,              // Heartbeat <0>
        TestRequest,            // Test Request <1>
        ResendRequest,          // Resend Request <2>
        Reject,                 // Reject <3>
        SequenceReset,          // Sequence Reset <4>
        Logout,                 // Logout <5>
        IndicationofInterest,   // Indication of Interest <6>
        Advertisement,          // Advertisement <7>
        ExecutionReport,        // Execution Report <8>
        OrderCancelReject,      // Order Cancel Reject <9>
        Logon,                  // Logon <A>
        News,                   // News <B>
        Email,                  // Email <C>
        OrderSingle,            // Order - Single <D>        
        OrderList,              // Order - List <E>
        OrderCancelRequest,     // Order Cancel Request <F>
        OrderCancelReplaceRequest, // Order Cancel/Replace Request <G>
        OrderStatusRequest,     // Order Status Request <H>
        AllocationInstruction,  // Allocation Instruction <J>
        ListCancelRequest,      // List Cancel Request <K>
        ListExecute,            // List Execute <L>
        ListStatusRequest,      // List Status Request <M>
        ListStatus,             // List Status <N>
        AllocationInstructionAck, // Allocation Instruction Ack <P>
        DontKnowTrade,          // Don't Know Trade <Q> (DK)
        QuoteRequest,           // Quote Request <R>
        Quote,                  // Quote <S>
        SettlementInstructions, // Settlement Instructions <T>
        MarketDataRequest,      // Market Data Request <V>
        MarketDataSnapshotFullRefresh, // Market Data-Snapshot/Full Refresh <W>
        MarketDataIncrementalRefresh,  // Market Data-Incremental Refresh <X>
        MarketDataRequestReject,// Market Data Request Reject <Y>
        QuoteCancel,            // Quote Cancel <Z>
        QuoteStatusRequest,     // Quote Status Request <a>
        MassQuoteAcknowledgement, // Mass Quote Acknowledgement <b>
        SecurityDefinitionRequest,// Security Definition Request <c>
        SecurityDefinition,     // Security Definition <d>
        SecurityStatusRequest,  // Security Status Request <e>
        SecurityStatus,         // Security Status <f>
        TradingSessionStatusRequest, // Trading Session Status Request <g>
        TradingSessionStatus,   // Trading Session Status <h>
        MassQuote,              // Mass Quote <i>
        BusinessMessageReject,  // Business Message Reject <j>
        BidRequest,             // Bid Request <k>
        BidResponse,            // Bid Response <l> (lowercase L)
        ListStrikePrice,        // List Strike Price <m>
        XMLmessage,             // XML message <n> (e.g. non-FIX MsgType <35>)
        RegistrationInstructions, // Registration Instructions <o>
        RegistrationInstructionsResponse, // Registration Instructions Response <p>
        OrderMassCancelRequest, // Order Mass Cancel Request <q>
        OrderMassCancelReport,  // Order Mass Cancel Report <r>
        NewOrderCross,          // New Order - Cross <s>
        CrossOrderCancelReplaceRequest, // Cross Order Cancel/Replace Request <t> (a.k.a. Cross Order Modification Request)
        CrossOrderCancelRequest,// Cross Order Cancel Request <u>
        SecurityTypeRequest,    // Security Type Request <v>
        SecurityTypes,          // Security Types <w>
        SecurityListRequest,    // Security List Request <x>
        SecurityList,           // Security List <y>
        DerivativeSecurityListRequest, // Derivative Security List Request <z>
        DerivativeSecurityList, // Derivative Security List <AA>
        NewOrderMultileg,       // New Order - Multileg
        MultilegOrderCancelReplace, // Multileg Order Cancel/Replace <AC> (a.k.a. Multileg Order Modification Request)
        TradeCaptureReportRequest, // Trade Capture Report Request <AD>
        TradeCaptureReport,     // Trade Capture Report <AE>
        OrderMassStatusRequest, // Order Mass Status Request <AF>
        QuoteRequestReject,     // Quote Request Reject <AG>
        RFQRequest,             // RFQ Request <AH>
        QuoteStatusReport,      // Quote Status Report <AI>
        QuoteResponse,          // Quote Response <AJ>
        Confirmation,           // Confirmation <AK>
        PositionMaintenanceRequest, // Position Maintenance Request <AL>
        PositionMaintenanceReport,  // Position Maintenance Report <AM>
        RequestForPositions,    // Request For Positions <AN>
        RequestForPositionsAck, // Request For Positions (AN) Ack <AO>
        PositionReport,         // Position Report <AP>
        TradeCaptureReportRequestAck, // Trade Capture Report Request Ack <AQ>
        TradeCaptureReportAck,  // Trade Capture Report Ack <AR>
        AllocationReport,       // Allocation Report <AS> (aka Allocation Claim)
        AllocationReportAck,    // Allocation Report Ack <AT> (aka Allocation Claim Ack)
        ConfirmationAck,        // Confirmation Ack <AU> (aka Affirmation)
        SettlementInstructionRequest, // Settlement Instruction Request <AV>
        AssignmentReport,       // Assignment Report <AW>
        CollateralRequest,      // Collateral Request <AX>
        CollateralAssignment,   // Collateral Assignment <AY>
        CollateralResponse,     // Collateral Response <AZ>
        CollateralReport,       // Collateral Report <BA>
        CollateralInquiry,      // Collateral Inquiry <BB>
        NetworkStatusRequest,   // Network (Counterparty System) Status Request <BC>
        NetworkStatusResponse,  // Network (Counterparty System) Status Response <BD>
        UserRequest,            // User Request <BE>
        UserResponse,           // User Response <BF>
        CollateralInquiryAck,   // Collateral Inquiry Ack <BG>
        ConfirmationRequest     // Confirmation Request <BH>
    }

    public static class FixMessageTypeFormatter
    {
        public static FixMessageType? GetMessageType(string _msgType)
        {
            FixMessageType fmt;
            if (msgType.TryGetValue(_msgType, out fmt)) return fmt;
            return null;
        }

        public static string GetMessageTypeString(FixMessageType _msgType)
        {
            foreach (var pair in msgType)
            {
                if (pair.Value == _msgType) return pair.Key;
            }
            return "";
        }

        public static Dictionary<string, FixMessageType> msgType =
            new Dictionary<string, FixMessageType>
                {
                    {"0", FixMessageType.Heartbeat},
                    {"1", FixMessageType.TestRequest},
                    {"2", FixMessageType.ResendRequest},
                    {"3", FixMessageType.Reject},
                    {"4", FixMessageType.SequenceReset},
                    {"5", FixMessageType.Logout},
                    {"6", FixMessageType.IndicationofInterest},
                    {"7", FixMessageType.Advertisement},
                    {"8", FixMessageType.ExecutionReport},
                    {"9", FixMessageType.OrderCancelReject},
                    {"A", FixMessageType.Logon},
                    {"B", FixMessageType.News},
                    {"C", FixMessageType.Email},
                    {"D", FixMessageType.OrderSingle},
                    {"E", FixMessageType.OrderList},
                    {"F", FixMessageType.OrderCancelRequest},
                    {"G", FixMessageType.OrderCancelReplaceRequest},
                    {"H", FixMessageType.OrderStatusRequest},
                    {"J", FixMessageType.AllocationInstruction},
                    {"K", FixMessageType.ListCancelRequest},
                    {"L", FixMessageType.ListExecute},
                    {"M", FixMessageType.ListStatusRequest},
                    {"N", FixMessageType.ListStatus},
                    {"P", FixMessageType.AllocationInstructionAck},
                    {"Q", FixMessageType.DontKnowTrade},
                    {"R", FixMessageType.QuoteRequest},
                    {"S", FixMessageType.Quote},
                    {"T", FixMessageType.SettlementInstructions},
                    {"V", FixMessageType.MarketDataRequest},
                    {"W", FixMessageType.MarketDataSnapshotFullRefresh},

                    {"X", FixMessageType.MarketDataIncrementalRefresh},
                    {"Y", FixMessageType.MarketDataRequestReject},
                    {"Z", FixMessageType.QuoteCancel},
                    {"a", FixMessageType.QuoteStatusRequest},
                    {"b", FixMessageType.MassQuoteAcknowledgement},
                    {"c", FixMessageType.SecurityDefinitionRequest},
                    {"d", FixMessageType.SecurityDefinition},
                    {"e", FixMessageType.SecurityStatusRequest},
                    {"f", FixMessageType.SecurityStatus},
                    {"g", FixMessageType.TradingSessionStatusRequest},
                    {"h", FixMessageType.TradingSessionStatus},
                    {"i", FixMessageType.MassQuote},
                    {"j", FixMessageType.BusinessMessageReject},
                    {"k", FixMessageType.BidRequest},
                    {"l", FixMessageType.BidResponse},
                    {"m", FixMessageType.ListStrikePrice},
                    {"n", FixMessageType.XMLmessage},
                    {"o", FixMessageType.RegistrationInstructions},
                    {"p", FixMessageType.RegistrationInstructionsResponse},
                    {"q", FixMessageType.OrderMassCancelRequest},
                    {"r", FixMessageType.OrderMassCancelReport},
                    {"s", FixMessageType.NewOrderCross},
                    
                    {"t", FixMessageType.CrossOrderCancelReplaceRequest},
                    {"u", FixMessageType.CrossOrderCancelRequest},
                    {"v", FixMessageType.SecurityTypeRequest},
                    {"w", FixMessageType.SecurityTypes},
                    {"x", FixMessageType.SecurityListRequest},
                    {"y", FixMessageType.SecurityList},
                    {"z", FixMessageType.DerivativeSecurityListRequest},
                    {"AA", FixMessageType.DerivativeSecurityList},
                    {"AB", FixMessageType.NewOrderMultileg},
                    {"AC", FixMessageType.MultilegOrderCancelReplace},
                    {"AD", FixMessageType.TradeCaptureReportRequest},
                    {"AE", FixMessageType.TradeCaptureReport},
                    {"AF", FixMessageType.OrderMassStatusRequest},
                    {"AG", FixMessageType.QuoteRequestReject},
                    {"AH", FixMessageType.RFQRequest},
                    {"AI", FixMessageType.QuoteStatusReport},
                    {"AJ", FixMessageType.QuoteResponse},
                    {"AK", FixMessageType.Confirmation},
                    {"AL", FixMessageType.PositionMaintenanceRequest},
                    {"AM", FixMessageType.PositionMaintenanceReport},
                    {"AN", FixMessageType.RequestForPositions},
                    {"AO", FixMessageType.RequestForPositionsAck},
                    {"AP", FixMessageType.PositionReport},
                    {"AQ", FixMessageType.TradeCaptureReportRequestAck},
                    {"AR", FixMessageType.TradeCaptureReportAck},
                    
                    {"AS", FixMessageType.AllocationReport},
                    {"AT", FixMessageType.AllocationReportAck},
                    {"AU", FixMessageType.ConfirmationAck},
                    {"AV", FixMessageType.SettlementInstructionRequest},
                    {"AW", FixMessageType.AssignmentReport},
                    {"AX", FixMessageType.CollateralRequest},
                    {"AY", FixMessageType.CollateralAssignment},
                    {"AZ", FixMessageType.CollateralResponse},
                    {"BA", FixMessageType.CollateralReport},
                    {"BB", FixMessageType.CollateralInquiry},
                    {"BC", FixMessageType.NetworkStatusRequest},
                    {"BD", FixMessageType.NetworkStatusResponse},
                    {"BE", FixMessageType.UserRequest},
                    {"BF", FixMessageType.UserResponse},
                    {"BG", FixMessageType.CollateralInquiryAck},
                    {"BH", FixMessageType.ConfirmationRequest},
                };
    }
}

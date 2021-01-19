namespace Reimers.Ihe.Communication.Tests
{
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public static class MiddlewareTests
    {
        public class GivenAMiddlewareWithATransactionHandler
        {
            private readonly DefaultHl7MessageMiddleware _defaultHl7MessageMiddleware;

            public GivenAMiddlewareWithATransactionHandler()
            {
                _defaultHl7MessageMiddleware = new DefaultHl7MessageMiddleware(
                    new TestTransactionHandler(),
                    new TestDischargeTransactionHandler(),
                    new TestOruTransactionHandler());
            }

            [Fact]
            public async Task WhenHandlingMessageThenReturnsResponse()
            {
                IMessageControlIdGenerator generator = DefaultMessageControlIdGenerator.Instance;
                var adt = new ADT_A01();
                adt.MSH.MessageControlID.Value = generator.NextId();

                var msg = new Hl7Message(adt, "");
                var response = await _defaultHl7MessageMiddleware.Handle(msg).ConfigureAwait(false);

                Assert.NotNull(response);
            }

            [Fact]
            public async Task WhenHandlingDischargeMessageThenReturnsResponse()
            {
                var parser = new PipeParser();
                var adt = parser.Parse(@"MSH|^~\&|AccMgr|1|||20050112154645||ADT^A03|59912415|P|2.5.1||| EVN|A03|20050112154642|||||
PID|1||10006579^^^1^MRN^1||DUCK^DONALD^D||19241010|M||1|111^DUCK ST^^FOWL^CA^999990000^^M|1|8885551212|8885551212|1|2||40007716^^^AccMgr^VN^1|123121234|||||||||||NO
PV1|1|I|IN1^214^1^1^^^S|3||IN1^214^1|37^DISNEY^WALT^^^^^^AccMgr^^^^CI|||01||||1|||37^DISNEY^WALT^^^^^^AccMgr^^^^CI|2|40007716^^^AccMgr^VN|4||||||||||||||||1|||1||P|||20050110045253|20050112152000|3115.89|3115.89|||");

                var msg = new Hl7Message(adt, "");
                var response = await _defaultHl7MessageMiddleware.Handle(msg).ConfigureAwait(false);

                Assert.NotNull(response);
            }

            [Fact]
            public async Task WhenHandlingOruMessageThenReturnsResponse()
            {
                var parser = new PipeParser();
                var adt = parser.Parse(@"MSH|^~\&|FDHL7|JOHNSON LABS||P1055|201007231634||ORU^R01|P10550000047907|P|2.5.1|1||NE|NE
PID|1|JQ4988|108512373||SAMPLES^JUNIOR||01/10/1948^53 Y|M|||^******^^||||||||
NTE|1|P|****************************************************************************
ADD|NON FASTING
OBR|1||108512373|CHEM^-* CHEMISTRY *||201007221041||||||||201007222312||P1055^SCI DULUTH/PHS^RTE 29,PO BOX 244^DULUTH, MN 19426^|(945)4431234|RECEPTION, NEW||||201007231634|||R||||
OBX|1|NM|01354^TotalProtein||7.3|gm/dl|5.98.4||||F
OBX|2|NM|00331^Albumin||3.9|gm/dl|3.25.2||||F
OBX|3|NM|17533^Globulin||3.4|gm/dL|1.73.7||||F
OBX|4|NM|06411^A/G Ratio||1.1||1.12.9||||F
OBX|5|NM|19760^Glucose||296|mg/dL|7099|HI|||F
OBX|6|NM|01487^Sodium||134|mmol/L|133145||||F
OBX|7|NM|01297^Potassium||4.3|mmol/L|3.35.3||||F
OBX|8|NM|00570^Chloride||96|mmol/L|96108||||F
OBX|9|NM|00521^CO2||24|mmol/L|2129||||F
OBX|10|NM|00497^BUN||17|mg/dl|725||||F
OBX|11|NM|00703^Creatinine||1.1|mg/dl|0.61.3||||F
OBX|12|NM|0900134^eGFR||70||>60 mL/min/1.73m2||||F
OBX|13|NM|14274^BUN/CreatRatio||15.5||1028||||F
OBX|14|NM|00505^Calcium||8.9|mg/dl|8.410.4||||F
OBX|15|NM|01578^UricAcid||6.2|mg/dl|2.47.0||||F
OBX|16|NM|01149^Iron||87|mcg/dl|30160||||F
OBX|17|NM|00430^Bilirubin,Total||0.6|mg/dl|0.11.0||||F
OBX|18|NM|01172^LDH||190|u/l|94250||||F
OBX|19|NM|01859^AlkPhos||63|u/l|39120||||F
OBX|20|NM|01461^AST (SGOT)||33|u/l|037||||F
OBX|21|NM|01271^Phosphorous||2.8|mg/dl|2.64.5||||F
OBX|22|NM|01479^ALT (SGPT)||55|u/L|040|HI|||F
OBX|23|NM|00935^GGTP||33|u/L|751||||F
NTE|1|L|****************************************************************************
ADD|GFR (GlomerularFiltrationRate) calculation utilizes the MDRD formula
ADD|(Modification of DietinRenalDiseaseStudyGroup)and assumes a normal
ADD|adult body surface area of 1.73. If the patient isAfricanAmerican
ADD|multiply result reported by1.21.(Ref.NationalKidneyDiseaseEduca.
ADD|Program.)
ADD| *****Male/Female reference range: >60 mL/min/1.73 m2 *****
ADD|Note: A calculated GFR of <60 mL suggests chronic kidney disease, but
ADD|only if found consistently over at least 3 months. A calculated
ADD|result of <15 mL is consistent with renal failure.
OBR|2||108512373|CARD^-* CARDIOVASCULAR/LIPIDS *||201007221041||||||||201007222312||P1055^SCI DULUTH/PHS^RTE 29,PO BOX 244^DULUTH, MN 19426^|(945)4431234|RECEPTION, NEW||||201007231634|||R||||
OBX|1|NM|00588^Cholesterol||124|mg/dl|<200||||F
OBX|2|NM|01552^Triglycerides||73|mg/dl|<151||||F
OBX|3|NM|00596^HDL CHOL.,DIRECT||39|mg/dl|>40|LO|||F
OBX|4|NM|17640^HDL as% of Cholesterol||31|%|||||F
NTE|1|L|Range/Evaluation: (>25) BELOW AVERAGE RISK
OBX|5|NM|14217^Chol/HDL Ratio||3.18||||||F
NTE|1|L|Range/Evaluation: (<4.2) BELOW AVERAGE RISK
OBX|6|NM|02535^LDL/HDL Ratio||1.82||03.55||||F
OBX|7|NM|05058^LDL Cholesterol||71||<100||||F
OBX|8|NM|33456^VLDL, CALCULATED||14|mg/dl|732||||F
OBR|3||108512373|HEMA^* HEMATOLOGY *||201007221041||||||||201007222312||P1055^SCI DULUTH/PHS^RTE 29,PO BOX 244^DULUTH, MN 19426^|(945)4431234|RECEPTION, NEW||||201007231634|||R||||
OBX|1|NM|14977^WBC||6.61|x10(3)/uL|3.4011.80||||F
OBX|2|NM|14985^RBC||4.56|x10(6)/uL|4.205.90||||F
OBX|3|NM|14993^HGB||13.6|gm/dL|12.317.0||||F
OBX|4|NM|00190^HCT||39.9|%|39.352.5||||F
OBX|5|NM|15032^MCV||87.5|fL|80.0100.0||||F
OBX|6|NM|15040^MCH||29.8|pg|25.034.1||||F
OBX|7|NM|15024^MCHC||34.1|gm/dL|29.035.0||||F
OBX|8|NM|15982^RDW||14.1|%|10.916.9||||F
OBX|9|NM|15057^POLYS||58.8|%|36.078.0||||F
OBX|10|NM|31765^POLYS, ABS. COUNT||3.89|x10(3)/uL|1.229.20||||F
OBX|11|NM|15073^LYMPHS||31.0|%|12.048.0||||F
OBX|12|NM|31773^LYMPHS, ABS. COUNT||2.05|x10(3)/uL|0.415.66||||F
OBX|13|NM|15115^MONOS||7.7|%|0.013.0||||F
OBX|14|NM|31807^MONOS, ABS. COUNT||0.51|x10(3)/uL|0.171.42||||F
OBX|15|NM|15099^EOS||2.0|%|0.08.0||||F
OBX|16|NM|31781^EOS, ABS. COUNT||0.13|x10(3)/uL|0.030.94||||F
OBX|17|NM|15107^BASOS||0.3|%|0.02.0||||F
OBX|18|NM|31799^BASOS, ABS. COUNT||0.02|x10(3)/uL|0.000.24||||F
OBX|19|NM|2700532^IMMATURE GRANULOCYTES||0.2|%|0.00.5||||F
OBX|20|NM|01289^PLATELET COUNT||191|x10(3)/uL|144400||||F
OBX|21|NM|4000535^MPV||10.6|fL|8.211.9||||F
OBR|4||108512373|URIN^* URINALYSIS *||201007221041||||||||201007222312||P1055^SCI DULUTH/PHS^RTE 29,PO BOX 244^DULUTH, MN 19426^|(945)4431234|RECEPTION, NEW||||201007231634|||R||||
OBX|1|ST|63156^Color||YELLOW||YELLOW, STRAW, AMBER||||F
OBX|2|ST|63164^Character||CLEAR||CLEAR||||F
OBX|3|NM|15206^SpecificGravity URN||1.030||1.0031.030||||F
OBX|4|NM|15214^pH Urine||5.5||5.08.0||||F
OBX|5|ST|15222^Protein,Urine||NEGATIVE||NEGATIVE||||F
OBX|6|ST|15230^Glucose,Urine||3+,>=1000 mg/dL||NEGATIVE|*|||F
OBX|7|ST|15248^Ketone,Urine||NEGATIVE||NEGATIVE||||F
OBX|8|NM|15255^UrobilinogenUrine||1.0|Units|0.21.0||||F
OBX|9|ST|15263^Bilirubin,Urine||NEGATIVE||NEGATIVE||||F
OBX|10|ST|15271^Blood,Urine||NEGATIVE||NEGATIVE||||F
OBX|11|ST|15289^NitritesUrine||NEGATIVE||NEGATIVE||||F
OBX|12|ST|63115^LeukocyteEsterase||NEGATIVE||NEGATIVE||||F
OBX|13|ST|15297^CrystalsUrine||NONE||NONE||||F
OBX|14|ST|21352^CrystalAmt.Urine||NONE||NONE||||F
OBX|15|ST|15347^WBC,Urine||04|PER HPF|04||||F
OBX|16|ST|15354^RBC,Urine||03|PER HPF|03||||F
OBX|17|ST|15461^EpithelialCells,Ur||FEW||FEW||||F
OBX|18|ST|15453^Cast,Hyaline,Urine||NONE SEEN|PER LPF|04||||F
OBX|19|ST|15479^Cast,Granular,Urin||NONE SEEN|PER LPF|01||||F
OBX|20|ST|15438^Cast, RBC,Urine||NONE SEEN|PER LPF|01||||F
OBX|21|ST|15495^Bacteria,Urine||NONE||FEW||||F
NTE|1|L|****************************************************************************
ADD|NOTE: Significant quantities of epithelial cells will
ADD|be identified if they are not squamous cell types.
OBR|5||108512373|MISC^* MISCELLANEOUS *||201007221041||||||||201007222312||P1055^SCI DULUTH/PHS^RTE 29,PO BOX 244^DULUTH, MN 19426^|(945)4431234|RECEPTION, NEW||||201007231634|||R||||
OBX|1|NM|01537^TSH||1.930||0.274.2 uIU/mL||||F
OBX|2|NM|01511^THYROXINE(T4)||9.3||4.512.0 ug/dL||||F
OBX|3|NM|01529^T3 UPTAKE||29.7||24.339.0%||||F
OBX|4|NM|06668^FREE T4 INDEX||2.8||1.14.5||||F
OBX|5|ST|01420^RPR||NONREACT||NONREACTIVE||||F
NTE|1|L|****************************************************************************
ADD|NOTICE: IF the result of the RPR is reported as reactive with a titer
ADD|of up to 1:8 please note that this level of reactivity can be caused
ADD|by other, nonspecific constituents and may not be related to syphilis.
ADD|Confirmation of positive RPRs can only be made via performance of the
ADD|T.Pallidum confirmation test.
OBX|6|NM|01024^HGB. A1c(glycohgb)||9.1||46%|HI|||F
OBX|7|ST|16618^CREAT.URN.TIMED/RAND||.147||gms/dL||||F
OBX|8|NM|26997^MICROALB/CREAT RATIO||4.1||<30mg/gm creat.||||F
OBX|9|NM|31724^MICROALBUMIN,RANDOM||0.6||<2.9 mg/dL||||F
NTE|2|L|****************************************************************************
ADD|GLYCOHEMOGLOBIN(HgbA1c)Ranges% eAG ranges(mg/dL)* GLUCOSE CONTROL INDEX
ADD|
ADD| <46% <68126 NonDiabeticLevel
ADD| <67% <126154 DiabeticControl
ADD| >8% >183 Additional action suggested
ADD|*Data adapted from the A1cDerivedAverageGlucose(ADAG)Study
ADD|(20062008).Estimated average glucose (eAG) values (shown as ranges
ADD|in the above table) can be reported as individual patient values if
ADD|requested.
NTE|3|L|****************************************************************************
ADD|NOTE: SST tube submitted was inadequately spun.Serum was found to
ADD|contain RBCs.Certain tests, e.g.Glucose, may be decreased while
ADD|others e.g.Potassiumor LDH may be elevated.");

                var msg = new Hl7Message(adt, "");
                var response = await _defaultHl7MessageMiddleware.Handle(msg).ConfigureAwait(false);

                Assert.NotNull(response);
            }
        }
    }
}
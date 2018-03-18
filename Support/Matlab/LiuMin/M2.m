clear,clc
%��������Ϊÿ̨������ͬ�ĵط�%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M2/M2LuckyPayout.txt','%d %s %s %d %d %f %f %s %d %s %s %s %s %f %s','headerlines',1);
%����Ϊ��������
ID=s1;
Symbols=s2;
PayoutType=s3;
Count=s4;
Ratio=s5;
OverallHit=s6;
FreeSpinOverallHit=s7;
FreeSpinHits=s8;
LongLuckySubtractFactor=s9;
IsFixed=s10;
Reel1Wild=s11;
Reel2Wild=s12;
Reel3Wild=s13;
StandardHit=s14;
IsShortLucky=s15;
%��������Ϊÿ̨������ͬ�ĵط�%

NumOfItem=length(s1);%�н������Ŀ��
TotalHitRate=sum(OverallHit);%��ͨ��תʱ�����н���
TrueFreeSpinOverallHit=zeros(NumOfItem,1);%FreeSpinʱ�����н���
for i=1:NumOfItem
    TrueFreeSpinOverallHit(i)=FreeSpinOverallHit(i)/sum(FreeSpinOverallHit);
end

%��������Ϊÿ̨������ͬ�ĵط�%
Reel1WildNum=zeros(NumOfItem,3);%����1�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:3
        Reel1WildNum(i,j)=str2num(string{j});
    end
end
Reel2WildNum=zeros(NumOfItem,4);%����2�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel2Wild{i},',');
    for j=1:4
        Reel2WildNum(i,j)=str2num(string{j});
    end
end
Reel3WildNum=zeros(NumOfItem,3);%����3�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel3Wild{i},',');
    for j=1:3
        Reel3WildNum(i,j)=str2num(string{j});
    end
end
%��������Ϊÿ̨������ͬ�ĵط�%

Expectation=zeros(NumOfItem,1);%��ͨ��תʱ������ֵ�ķֲ���
ExpectationInFreeSpin=zeros(NumOfItem,1);%FreeSpinʱ������ֵ�ķֲ���
PBonusWild=zeros(NumOfItem,1);%BonusWild�ĸ��ʷֲ���
for i=1:9
    %Ordered3
    Expectation(i)=OverallHit(i)*Ratio(i);
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i);
    PBonusWild(i)=OverallHit(i);
end
for i=10:16
    %All3
    %��ÿ���������Ƿ��滻ΪWild�����࣬1��ʾ�滻Ϊwild����8�������(000��001��010��011��100��101��110��111)�����в�����111���������111ת��Ϊ110��101��011
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R2=Reel2WildNum(i,1)+Reel2WildNum(i,2)+Reel2WildNum(i,3)+Reel2WildNum(i,4);
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33;
    %��������Ϊÿ̨������ͬ�ĵط�%
    P111=R1*R2*R3;%��һ����������������Wild����Ϊ����������������������ĸ��ʱ�ƽ�����䵽110��101��011���������
    P000=(1-R1)*(1-R2)*(1-R3);%��һ��������������û��Wild��i.e. StandardHit
    P001=(1-R1)*(1-R2)*R3;%������������Wild
    P010=(1-R1)*R2*(1-R3);%�ڶ���������Wild
    P011=(1-R1)*R2*R3+1/3*P111;%�ڶ�������������Wild,��������P111ת�������Ĳ���
    P100=R1*(1-R2)*(1-R3);%��һ��������Wild
    P101=R1*(1-R2)*R3+1/3*P111;%��һ������������Wild,��������P111ת�������Ĳ���
    P110=R1*R2*(1-R3)+1/3*P111;%��һ������������Wild,��������P111ת�������Ĳ���
    PAll3=P000+P001+P010+P011+P100+P101+P110;%%��֤����7������ĸ���֮���Ƿ�Ϊ1
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAll3=0;%��ΪWild�������ı�����������StandardHit�����
    WildMultipleInAll3=WildMultipleInAll3+P000*1;%���000�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P001*(R31*5+R32*3+R33*1)/R3;%���001�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P010*(R2*1)/R2;%���010�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P011*(R2*1)/R2*(R31*5+R32*3+R33*1)/R3;%���011�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P100*(R11*5+R12*3+R13*1)/R1;%���100�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P101*(R11*5+R12*3+R13*1)/R1*(R31*5+R32*3+R33*1)/R3;%���101�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P110*(R11*5+R12*3+R13*1)/R1*(R2*1)/R2;%���110�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i)*WildMultipleInAll3;
    PBonusWild(i)=OverallHit(i)*(P010*R2+P011*R2+P110*R2);
end
for i=17:18
    %Any3,SymbolsType=4��
    %��4^3=64��Symbol���У���4��symbol�ֱ�ΪA��B��C��D
    %(AAA,BBB,CCC,DDD)=(4��1/64)=>(111)=1/64*4=1/16=>(111->1/3(110),1/3(101),1/3(011))=>(110,101,011)=(1/48,1/48,1/48)
    %(AAB,AAC,AAD,BBA,BBC,BBD,CCA,CCB,CCD,DDA,DDB,DDC)=(12��1/64)=>(110)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABA,ACA,ADA,BAB,BCB,BDB,CAC,CBC,CDC,DAD,DBD,DCD)=(12��1/64)=>(101)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABB,ACC,ADD,BAA,BCC,BDD,CAA,CBB,CDD,DAA,DBB,DCC)=(12��1/64)=>(011)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABC,ABD,ACB,ACD,ADB,ADC,BAC,BAD,BCA,BCD,BDA,BDC,CAB,CAD,CBA,CBD,CDA,CDB,DAB,DAC,DBA,DBC,DCA,DCB)=(24��1/64)=>(111)=1/64*24=3/8
    %�����������ϵ��Ƿ�����ͬԪ�������ࣨ�������������ϵ�Ԫ���ܷ��滻Ϊwild�����ࣩ��ֻ����1�ϲ��п����滻Ϊwild������һ���滻Ϊwild������4���������110��101��011,111��=��1/4,3/16,3/16,3/8��.
    WildPossibility110=5/24;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility101=5/24;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility011=5/24;%�ڶ����������symbol��ͬ�ĸ��ʣ�ֻ�еڶ������������滻Ϊwild�Ŀ����ԣ�
    WildPossibility111=3/8;%��һ�������������symbol������ͬ�ĸ��ʣ���һ�����������ᶼ�п����滻Ϊwild��
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R2=Reel2WildNum(i,1)+Reel2WildNum(i,2)+Reel2WildNum(i,3)+Reel2WildNum(i,4);
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33; 
    %��������Ϊÿ̨������ͬ�ĵط�%
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3)+WildPossibility111*(1-R1)*(1-R2)*(1-R3);%���(R1��R2��+R1��R3��+R2��R3��+R1'R2'R3'), ��ʾ��һ�����������ᶼ���滻Ϊwild�ĸ���.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3)+WildPossibility111*(1/3*R1+1/3*(1-R2)*1/2*R1+1/3*(1-R2)*1/2*(1-R3)*R1+1/3*(1-R3)*1/2*R1+1/3*(1-R3)*1/2*(1-R2)*R1);%���(R1R2'+R1R3'+R1R2'R3'),��ʾֻ�е�һ�������滻Ϊwild�ĸ���.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3)+WildPossibility111*(1/3*R2+1/3*(1-R1)*1/2*R2+1/3*(1-R1)*1/2*(1-R3)*R2+1/3*(1-R3)*1/2*R2+1/3*(1-R3)*1/2*(1-R1)*R2);%���(R2R1'+R2R3'+R2R1'R3'),��ʾֻ�еڶ��������滻Ϊwild�ĸ���.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3)+WildPossibility111*(1/3*R3+1/3*(1-R1)*1/2*R3+1/3*(1-R1)*1/2*(1-R2)*R3+1/3*(1-R2)*1/2*R3+1/3*(1-R2)*1/2*(1-R1)*R3);%���(R3R1'+R3R2'+R3R1'R2'),��ʾֻ�е����������滻Ϊwild�ĸ���.
    PAny3SymbolsType3=P000+P100+P010+P001;%%��֤����4������ĸ���֮���Ƿ�Ϊ1
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAny3Symbols4=0;%��ΪWild�������ı���������StandardHit�����
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P000*1;%���000�����Ļ�����������StandardHit�Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P100*(R11*5+R12*3+R13*1)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P010*(R2*1)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P001*(R31*5+R32*3+R33*1)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;  
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;
    PBonusWild(i)=OverallHit(i)*P010*R2;
end

%����ΪFreeSpin�淨�����Ĵ���%
FreeSpinHitsNum=zeros(NumOfItem,4);%ÿ�����FreeSpin�������ñ�
FreeSpin=zeros(NumOfItem,100);%ÿ�����FreeSpin X�εĸ���
FreeSpinTimes=zeros(NumOfItem,1);%ÿ�����FreeSpin����������ֵ,������BonusWild���ֵĸ���
FreeSpinTimesExpectation=zeros(NumOfItem,1);%ÿ�����FreeSpin�ܴ���������ֵ������BonusWild���ֵĸ���
for i=1:NumOfItem
    string=strsplit(FreeSpinHits{i},',');
    for j=1:4
        FreeSpinHitsNum(i,j)=str2num(string{j});
    end
    FreeSpin(i,1)=FreeSpinHitsNum(i,1)*(1-FreeSpinHitsNum(i,2));%FreeSpin 1�εĸ���
    FreeSpin(i,2)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*(1-FreeSpinHitsNum(i,3));%FreeSpin 2�εĸ���
    FreeSpin(i,3)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)*(1-FreeSpinHitsNum(i,4));%FreeSpin 3�εĸ���
%     FreeSpin(i,4)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^2*(1-FreeSpinHitsNum(i,3));%FreeSpin 4�εĸ���
%     FreeSpin(i,5)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^3*(1-FreeSpinHitsNum(i,3));%FreeSpin 5�εĸ���
%     FreeSpin(i,6)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^4*(1-FreeSpinHitsNum(i,3));%FreeSpin 6�εĸ���
%     FreeSpin(i,7)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^5*(1-FreeSpinHitsNum(i,3));%FreeSpin 7�εĸ���
%     FreeSpin(i,8)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^6*(1-FreeSpinHitsNum(i,3));%FreeSpin 8�εĸ���
    for m=4:100
        FreeSpin(i,m)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)*FreeSpinHitsNum(i,4)^(m-3)*(1-FreeSpinHitsNum(i,4));%FreeSpin m�εĸ���
    end
    for j=1:100
        FreeSpinTimes(i)=FreeSpinTimes(i)+FreeSpin(i,j)*j;
    end
    FreeSpinTimesExpectation(i)=FreeSpinTimes(i)*PBonusWild(i);
end

TotalPBonusWild=sum(PBonusWild);%�������BonusWild���ֵĸ���֮�ͣ�ȱ��loss������³���wild�ĸ��ʣ�
TotalFreeSpinTimesExpectation=sum(FreeSpinTimesExpectation);%�������FreeSpin�ܴ���������ֵ
TotalExpectation=sum(Expectation);%���������ͨ��תʱ��������ֵ
TotalExpectationInFreeSpin=sum(ExpectationInFreeSpin);%�������FreeSpinʱ��������ֵ
TotalExpectationAfterFreeSpin=sum(Expectation)+sum(ExpectationInFreeSpin)*TotalFreeSpinTimesExpectation;%������ֵ

fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M2/M2LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectationInNormalSpin %f\n',TotalExpectation);
fprintf(fid,'TotalExpectationInFreeSpin %f\n',TotalExpectationInFreeSpin);
fprintf(fid,'TotalExpectationAfterFreeSpin %f\n',TotalExpectationAfterFreeSpin);
fprintf(fid,'TotalHitRateInNormalSpin %f\n',TotalHitRate);
fprintf(fid,'TotalPBonusWild %f\n',TotalPBonusWild);
fprintf(fid,'TotalFreeSpinTimesExpectation %f\n',TotalFreeSpinTimesExpectation);
fclose(fid);
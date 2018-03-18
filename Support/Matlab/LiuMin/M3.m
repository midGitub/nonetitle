clear,clc
%��������Ϊÿ̨������ͬ�ĵط�%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M3/M3LuckyPayout.txt','%d %s %s %d %f %s %f %d %s %s %s %s %f %s','headerlines',1);
%����Ϊ��������
ID=s1;
Symbols=s2;
PayoutType=s3;
Count=s4;
Ratio=s5;
RewindHits=s6;
OverallHit=s7;
LongLuckySubtractFactor=s7;
IsFixed=s9;
Reel1Wild=s10;
Reel2Wild=s11;
Reel3Wild=s12;
StandardHit=s13;
IsShortLucky=s14;
%��������Ϊÿ̨������ͬ�ĵط�%

NumOfItem=length(s1);%�н������Ŀ��
TotalHitRate=sum(OverallHit);%���н���

%��������Ϊÿ̨������ͬ�ĵط�%
Reel1WildNum=zeros(NumOfItem,2);%����1�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:2
        Reel1WildNum(i,j)=str2num(string{j});
    end
end
Reel2WildNum=zeros(NumOfItem,2);%����2�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel2Wild{i},',');
    for j=1:2
        Reel2WildNum(i,j)=str2num(string{j});
    end
end
Reel3WildNum=zeros(NumOfItem,2);%����3�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel3Wild{i},',');
    for j=1:2
        Reel3WildNum(i,j)=str2num(string{j});
    end
end
%��������Ϊÿ̨������ͬ�ĵط�%

Expectation=zeros(NumOfItem,1);%������Rewind������ֵ�ķֲ���
for i=1:2 
    %Ordered3
    Expectation(i)=OverallHit(i)*Ratio(i);
    StandardHit(i)=0;
end
for i=4:8
    %All3
    %��ÿ���������Ƿ��滻ΪWild�����࣬1��ʾ�滻Ϊwild����8�������(000��001��010��011��100��101��110��111)�����в�����111���������111ת��Ϊ110��101��011
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);
    R1=R11+R12;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);
    R2=R21+R22;
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);
    R3=R31+R32;
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
    StandardHit(i)=P000;
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAll3=0;%��ΪWild�������ı�����������StandardHit�����
    WildMultipleInAll3=WildMultipleInAll3+P000*1;%���000�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P001*(R31*3+R32*2)/R3;%���001�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P010*(R21*3+R22*2)/R2;%���010�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P011*(R21*3+R22*2)/R2*(R31*3+R32*2)/R3;%���011�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P100*(R11*3+R12*2)/R1;%���100�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P101*(R11*3+R12*2)/R1*(R31*3+R32*2)/R3;%���101�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P110*(R11*3+R12*2)/R1*(R21*3+R22*2)/R2;%���110�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
end
for i=9:10 
    %Any3,SymbolsType=2 
    %��2^3=8�����У���3��symbol�ֱ�ΪA��B��C
    %(AAA->1/3AAB+1/3ABA+1/3BAA,AAB,ABA,ABB,BAA,BAB,BBA,BBB->1/3BBA+1/3BAB+1/3ABB)=>(AAB,ABA,ABB,BAA,BAB,BBA)=(1/6,1/6,1/6,1/6,1/6,1/6)
    %�����������ϵ��Ƿ�����ͬԪ�������ࣨ�������������ϵ�Ԫ���ܷ��滻Ϊwild�����ࣩ��ֻ����1�ϲ��п����滻Ϊwild������һ���滻Ϊwild������3���������110��101��011��=(1/2,1/4,1/4��
    WildPossibility110=1/3;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility101=1/3;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility011=1/3;%�ڶ����������symbol��ͬ�ĸ��ʣ�ֻ�еڶ������������滻Ϊwild�Ŀ����ԣ�
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);
    R1=R11+R12;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);
    R2=R21+R22;
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);
    R3=R31+R32;
    %��������Ϊÿ̨������ͬ�ĵط�%
    %���(R1R2->1/2R1R2'+1/2R1'R2)
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3);%���(R1��R2��+R1��R3��+R2��R3��), ��ʾ��һ�����������ᶼ���滻Ϊwild�ĸ���.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3);%���(R1R2'+R1R3'),��ʾֻ�е�һ�������滻Ϊwild�ĸ���.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3);%���(R2R1'+R2R3'),��ʾֻ�еڶ��������滻Ϊwild�ĸ���.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3);%���(R3R1'+R3R2'),��ʾֻ�е����������滻Ϊwild�ĸ���.
    PAny3SymbolsType2=P000+P100+P010+P001;%%��֤����4������ĸ���֮���Ƿ�Ϊ1
    StandardHit(i)=P000;
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAny3Symbols2=0;%��ΪWild�������ı���������StandardHit�����
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P000*1;%���000�����Ļ�����������StandardHit�Ļ�������
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P100*(R11*3+R12*2)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P010*(R21*3+R22*2)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P001*(R31*3+R32*2)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols2;  
end
for i=11
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
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);
    R1=R11+R12;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);
    R2=R21+R22;
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);
    R3=R31+R32;
    %��������Ϊÿ̨������ͬ�ĵط�%
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3)+WildPossibility111*(1-R1)*(1-R2)*(1-R3);%���(R1��R2��+R1��R3��+R2��R3��+R1'R2'R3'), ��ʾ��һ�����������ᶼ���滻Ϊwild�ĸ���.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3)+WildPossibility111*(1/3*R1+1/3*(1-R2)*1/2*R1+1/3*(1-R2)*1/2*(1-R3)*R1+1/3*(1-R3)*1/2*R1+1/3*(1-R3)*1/2*(1-R2)*R1);%���(R1R2'+R1R3'+R1R2'R3'),��ʾֻ�е�һ�������滻Ϊwild�ĸ���.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3)+WildPossibility111*(1/3*R2+1/3*(1-R1)*1/2*R2+1/3*(1-R1)*1/2*(1-R3)*R2+1/3*(1-R3)*1/2*R2+1/3*(1-R3)*1/2*(1-R1)*R2);%���(R2R1'+R2R3'+R2R1'R3'),��ʾֻ�еڶ��������滻Ϊwild�ĸ���.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3)+WildPossibility111*(1/3*R3+1/3*(1-R1)*1/2*R3+1/3*(1-R1)*1/2*(1-R2)*R3+1/3*(1-R2)*1/2*R3+1/3*(1-R2)*1/2*(1-R1)*R3);%���(R3R1'+R3R2'+R3R1'R2'),��ʾֻ�е����������滻Ϊwild�ĸ���.
    PAny3SymbolsType3=P000+P100+P010+P001;%%��֤����4������ĸ���֮���Ƿ�Ϊ1
    StandardHit(i)=P000;
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAny3Symbols4=0;%��ΪWild�������ı���������StandardHit�����
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P000*1;%���000�����Ļ�����������StandardHit�Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P100*(R11*3+R12*2)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P010*(R21*3+R22*2)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P001*(R31*3+R32*2)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;  
end
TotalExpectation=sum(Expectation);%������ֵ

%����ΪRewind�淨�����ı���%
Repay=zeros(NumOfItem,8);%ÿ�����Rewind X�εĸ���
TotalRepay=zeros(NumOfItem,1);%ÿ�����Rewind����������ֵ
RewindHitsNum=zeros(NumOfItem,3);%ÿ�����Rewind�������ñ�
for i=1:NumOfItem
    string=strsplit(RewindHits{i},',');
    for j=1:3
        RewindHitsNum(i,j)=str2num(string{j});
    end
    Repay(i,1)=RewindHitsNum(i,1)*(1-RewindHitsNum(i,2));%Rewind 1�εĸ���
    Repay(i,2)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*(1-RewindHitsNum(i,3));%Rewind 2�εĸ���
    Repay(i,3)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*(1-RewindHitsNum(i,3));%Rewind 3�εĸ���
%     Repay(i,4)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*RewindHitsNum(i,4)^1*(1-RewindHitsNum(i,4));%Rewind 4�εĸ���
%     Repay(i,5)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*RewindHitsNum(i,4)^2*(1-RewindHitsNum(i,4));%Rewind 5�εĸ���
%     Repay(i,6)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*RewindHitsNum(i,4)^3*(1-RewindHitsNum(i,4));%Rewind 6�εĸ���
%     Repay(i,7)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*RewindHitsNum(i,4)^4*(1-RewindHitsNum(i,4));%Rewind 7�εĸ���
%     Repay(i,8)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)*RewindHitsNum(i,4)^5*(1-RewindHitsNum(i,4));%Rewind 8�εĸ���
    for m=4:100
        Repay(i,m)=RewindHitsNum(i,1)*RewindHitsNum(i,2)*RewindHitsNum(i,3)^(m-2)*(1-RewindHitsNum(i,3));%Rewind m�εĸ���
    end
    for j=1:100
        TotalRepay(i)=TotalRepay(i)+Repay(i,j)*j;
    end
end

ExpectationAfterRewind=zeros(NumOfItem,1);%����Rewind֮�������ֵ�ķֲ���
for i=1:NumOfItem
    ExpectationAfterRewind(i)=Expectation(i)*(1+TotalRepay(i));
end
TotalExpectationAfterRewind=sum(ExpectationAfterRewind);%����Rewind֮���������ֵ
TotalStandardHit=0;%�ܱ�׼�н�����wild������
for i=1:NumOfItem
    TotalStandardHit=TotalStandardHit+StandardHit(i)*OverallHit(i);
end

fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M3/M3LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectationBeforeRewind %f\n',TotalExpectation);
fprintf(fid,'TotalExpectationAfterRewind %f\n',TotalExpectationAfterRewind);
fprintf(fid,'TotalHitRate %f\n',TotalHitRate);
fprintf(fid,'TotalStandardHit %f\n',TotalStandardHit);
fprintf(fid,'TotalStandardHit/TotalHitRate %f\n',TotalStandardHit/TotalHitRate);
fprintf(fid,'TotalRepayTimes:\n');
for i=1:NumOfItem
    fprintf(fid,'ID=%d: %f\n',i,TotalRepay(i));
end
fclose(fid);
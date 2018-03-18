clear,clc
%��������Ϊÿ̨������ͬ�ĵط�%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15,s16]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M5/M5LuckyPayout.txt','%d %s %s %d %f %f %d %s %s %s %s %f %f %f %f %s','headerlines',1);
%����Ϊ��������
ID=s1;
Symbols=s2;
PayoutType=s3;
Count=s4;
Ratio=s5;
OverallHit=s6;
LongLuckySubtractFactor=s7;
IsFixed=s8;
Reel1Wild=s9;
Reel2Wild=s10;
Reel3Wild=s11;
StandardHit=s12;
Slide1=s13;
Slide2=s14;
Slide3=s15;
IsShortLucky=s16;
%��������Ϊÿ̨������ͬ�ĵط�%

NumOfItem=length(s1);%�н������Ŀ��
TotalHitRate=sum(OverallHit);%���н���

%��������Ϊÿ̨������ͬ�ĵط�%
Reel1WildNum=zeros(NumOfItem,3);%����1�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:3
        Reel1WildNum(i,j)=str2num(string{j});
    end
end
Reel2WildNum=zeros(NumOfItem,3);%����2�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel2Wild{i},',');
    for j=1:3
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

Expectation=zeros(NumOfItem,1);%����ֵ�ķֲ���
WildMultiplier=zeros(NumOfItem,1);%ÿ�������Wild�������ı���
for i=1:3 
    %Ordered3
    WildMultiplier(i)=1;
    Expectation(i)=OverallHit(i)*Ratio(i);
    StandardHit(i)=0;
end
for i=4:8
    %All3
    %��ÿ���������Ƿ��滻ΪWild�����࣬1��ʾ�滻Ϊwild����8�������(000��001��010��011��100��101��110��111)�����в�����111���������111ת��Ϊ110��101��011
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);%%Ϊ��������ļ��㣬���Լ򻯱�����
    R1=R11+R12+R13;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
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
    StandardHit(i)=P000;
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAll3=0;%��ΪWild�������ı�����������StandardHit�����    
    WildMultipleInAll3=WildMultipleInAll3+P000*1;%���000�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P001*(R3*5)/R3;%���001�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P010*(R21*15+R22*10+R23*5)/R2;%���010�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P011*(R21*15+R22*10+R23*5)/R2*(R3*5)/R3;%���011�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P100*(R1*5)/R1;%���100�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P101*(R1*5)/R1*(R3*5)/R3;%���101�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P110*(R1*5)/R1*(R21*15+R22*10+R23*5)/R2;%���110�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultiplier(i)=WildMultipleInAll3;
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
end
for i=9 
    %Any3,SymbolsType=2 
    %��2^3=8�����У���3��symbol�ֱ�ΪA��B��C
    %(AAA->1/3AAB+1/3ABA+1/3BAA,AAB,ABA,ABB,BAA,BAB,BBA,BBB->1/3BBA+1/3BAB+1/3ABB)=>(AAB,ABA,ABB,BAA,BAB,BBA)=(1/6,1/6,1/6,1/6,1/6,1/6)
    %�����������ϵ��Ƿ�����ͬԪ�������ࣨ�������������ϵ�Ԫ���ܷ��滻Ϊwild�����ࣩ��ֻ����1�ϲ��п����滻Ϊwild������һ���滻Ϊwild������3���������110��101��011��=(1/2,1/4,1/4��
    WildPossibility110=1/3;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility101=1/3;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility011=1/3;%�ڶ����������symbol��ͬ�ĸ��ʣ�ֻ�еڶ������������滻Ϊwild�Ŀ����ԣ�
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33;
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
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P100*(R1*5)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P010*(R21*15+R22*10+R23*5)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols2=WildMultipleInAny3Symbols2+P001*(R3*5)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultiplier(i)=WildMultipleInAny3Symbols2;
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols2;  
end
for i=10 
    %Any3,SymbolsType=3
    %��3^3=27������
    %(AAA,BBB,CCC)=(1/27,1/27,1/27)=>(111)=1/27*3=1/9=>(111->1/3(110)+1/3(101)+1/3(011))=>(110,101,011)=(1/27,1/27,1/27)
    %(AAB,AAC,BBA,BBC,CCA,CCB)=(1/27,1/27,1/27,1/27,1/27,1/27)=>(101)=1/27*6=2/9=>(110)=2/9+1/27=7/27
    %(ABA,ACA,BAB,BCB,CAC,CBC)=(1/27,1/27,1/27,1/27,1/27,1/27)=>(101)=1/27*6=2/9=>(101)=2/9+1/27=7/27
    %(ABB,ACC,BAA,BCC,CAA,CBB)=(1/27,1/27,1/27,1/27,1/27,1/27)=>(011)=1/27*6=2/9=>(011)=2/9+1/27=7/27
    %(ABC,ACB,BAC,BCA,CAB,CBA)=(1/27,1/27,1/27,1/27,1/27,1/27)=>(111)=1/27*6=2/9
    %���Ƿ���滻Ϊwild�����ࣨ110��101��011,111��=��7/27,7/27,7/27,2/9��.1�����滻Ϊwild. 
    WildPossibility110=7/27;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility101=7/27;%��һ���������symbol��ͬ�ĸ��ʣ�ֻ�е�һ�����������滻Ϊwild�Ŀ����ԣ�
    WildPossibility011=7/27;%�ڶ����������symbol��ͬ�ĸ��ʣ�ֻ�еڶ������������滻Ϊwild�Ŀ����ԣ�
    WildPossibility111=2/9;%��һ�������������symbol������ͬ�ĸ��ʣ���һ�����������ᶼ�п����滻Ϊwild��
    %��������Ϊÿ̨������ͬ�ĵط�%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33;
    %��������Ϊÿ̨������ͬ�ĵط�%
    %���(R1R2->1/2R1R2'+1/2R1'R2)
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3)+WildPossibility111*(1-R1)*(1-R2)*(1-R3);%���(R1��R2��+R1��R3��+R2��R3��+R1'R2'R3'), ��ʾ��һ�����������ᶼ���滻Ϊwild�ĸ���.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3)+WildPossibility111*(1/3*R1+1/3*(1-R2)*1/2*R1+1/3*(1-R2)*1/2*(1-R3)*R1+1/3*(1-R3)*1/2*R1+1/3*(1-R3)*1/2*(1-R2)*R1);%���(R1R2'+R1R3'+R1R2'R3'),��ʾֻ�е�һ�������滻Ϊwild�ĸ���.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3)+WildPossibility111*(1/3*R2+1/3*(1-R1)*1/2*R2+1/3*(1-R1)*1/2*(1-R3)*R2+1/3*(1-R3)*1/2*R2+1/3*(1-R3)*1/2*(1-R1)*R2);%���(R2R1'+R2R3'+R2R1'R3'),��ʾֻ�еڶ��������滻Ϊwild�ĸ���.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3)+WildPossibility111*(1/3*R3+1/3*(1-R1)*1/2*R3+1/3*(1-R1)*1/2*(1-R2)*R3+1/3*(1-R2)*1/2*R3+1/3*(1-R2)*1/2*(1-R1)*R3);%���(R3R1'+R3R2'+R3R1'R2'),��ʾֻ�е����������滻Ϊwild�ĸ���.
    PAny3SymbolsType3=P000+P100+P010+P001;%%��֤����4������ĸ���֮���Ƿ�Ϊ1 
    StandardHit(i)=P000;
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultipleInAny3Symbols3=0;%��ΪWild�������ı���������StandardHit�����
    WildMultipleInAny3Symbols3=WildMultipleInAny3Symbols3+P000*1;%���000�����Ļ�����������StandardHit�Ļ�������
    WildMultipleInAny3Symbols3=WildMultipleInAny3Symbols3+P100*(R1*5)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols3=WildMultipleInAny3Symbols3+P010*(R21*15+R22*10+R23*5)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols3=WildMultipleInAny3Symbols3+P001*(R3*5)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    WildMultiplier(i)=WildMultipleInAny3Symbols3;
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols3;
end
TotalExpectation=sum(Expectation);%������ֵ
TotalStandardHit=0;%�ܱ�׼�н�����wild������
for i=1:NumOfItem
    TotalStandardHit=TotalStandardHit+StandardHit(i)*OverallHit(i);
end
fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M5/M5LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectation %f\n',TotalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalHitRate);
fprintf(fid,'TotalStandardHit %f\n',TotalStandardHit);
fprintf(fid,'TotalStandardHit/TotalHitRate %f\n',TotalStandardHit/TotalHitRate);
fclose(fid);

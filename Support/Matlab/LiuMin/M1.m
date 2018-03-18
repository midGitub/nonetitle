clear,clc
%��������Ϊÿ̨������ͬ�ĵط�%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M1/M1LuckyPayout.txt','%d %s %s %d %f %f %d %s %s %s %s %f %s','headerlines',1);
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
IsShortLucky=s13;
%��������Ϊÿ̨������ͬ�ĵط�%

NumOfItem=length(s1);%�н������Ŀ��
TotalHitRate=sum(OverallHit);%���н���

%��������Ϊÿ̨������ͬ�ĵط�%
Reel1WildNum=zeros(NumOfItem,4);%����1�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:4
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
Reel3WildNum=zeros(NumOfItem,4);%����3�ϵ�Wild���ʷֲ���
for i=1:NumOfItem
    string=strsplit(Reel3Wild{i},',');
    for j=1:4
        Reel3WildNum(i,j)=str2num(string{j});
    end
end
%��������Ϊÿ̨������ͬ�ĵط�%

Expectation=zeros(NumOfItem,1);%����ֵ�ķֲ���
for i=1:3
    %Ordered3
    Expectation(i)=OverallHit(i)*Ratio(i);
    StandardHit(i)=0;
end
for i=4:10
    %All3
    %��ÿ���������Ƿ��滻ΪWild�����࣬1��ʾ�滻Ϊwild����8�������(000��001��010��011��100��101��110��111)�����в�����111���������111ת��Ϊ110��101��011
    %��������Ϊÿ̨������ͬ�ĵط�%
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%Ϊ��������ļ��㣬���Լ򻯱�����
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
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
    WildMultipleInAll3=WildMultipleInAll3+P001*(R3*2)/R3;%���001�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P010*(R21*3+R22*5+R23*10)/R2;%���010�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P011*(R21*3+R22*5+R23*10)/R2*(R3*2)/R3;%���011�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P100*(R1*2)/R1;%���100�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P101*(R1*2)/R1*(R3*2)/R3;%���101�����Ļ�������
    WildMultipleInAll3=WildMultipleInAll3+P110*(R1*2)/R1*(R21*3+R22*5+R23*10)/R2;%���110�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
end
for i=11:12
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
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%Ϊ��������ļ��㣬���Լ򻯱�����
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
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
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P100*(R1*2)/R1;%���100�����Ļ�������������һ�������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P010*(R21*3+R22*5+R23*10)/R2;%���010�����Ļ������������ڶ��������wild�����Ļ�������
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P001*(R3*2)/R3;%���001�����Ļ����������������������wild�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;  
end
for i=13 
    %Any2
    StandardHit(i)=0;
    %����������������wildԪ�������࣬��3�������(R1R2,R1R3,R2R3)=(1/3,1/3,1/3)
    WildExistence110=1/3;%��һ���������symbolΪwild�ĸ���
    WildExistence101=1/3;%��һ���������symbolΪwild�ĸ���
    WildExistence011=1/3;%�ڶ����������symbolΪwild�ĸ���
    %��������Ϊÿ̨������ͬ�ĵط�%
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%Ϊ��������ļ��㣬���Լ򻯱�����
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    WildMultipleInAny2=0;%��ΪWild�������ı�����û��StandardHit�����
    WildMultipleInAny2=WildMultipleInAny2+WildExistence110*(R1*2)/R1*(R21*3+R22*5+R23*10)/R2;%���110�����Ļ�������
    WildMultipleInAny2=WildMultipleInAny2+WildExistence101*(R1*2)/R1*(R3*2)/R3;%���101�����Ļ�������
    WildMultipleInAny2=WildMultipleInAny2+WildExistence011*(R21*3+R22*5+R23*10)/R2*(R3*2)/R3;%���011�����Ļ�������
    %��������Ϊÿ̨������ͬ�ĵط�%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny2;
end
for i=14
    %Any1
    StandardHit(i)=0;
    %����һ����������wildԪ�������࣬��3�������(R2,R2,R3)=(1/3,1/3,1/3)
    WildExistence100=1/3;%��һ�����symbolΪwild�ĸ���
    WildExistence010=1/3;%�ڶ������symbolΪwild�ĸ���
    WildExistence001=1/3;%���������symbolΪwild�ĸ���
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%Ϊ��������ļ��㣬���Լ򻯱�����
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    WildMultipleInAny1=0;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence100*(R1*2)/R1;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence010*(R21*3+R22*5+R23*10)/R2;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence001*(R3*2)/R3;
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny1;
end
TotalExpectation=sum(Expectation);%������ֵ
TotalStandardHit=0;%�ܱ�׼�н�����wild������
for i=1:NumOfItem
    TotalStandardHit=TotalStandardHit+StandardHit(i)*OverallHit(i);
end

fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M1/M1LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectation %f\n',TotalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalHitRate);
fprintf(fid,'TotalStandardHit %f\n',TotalStandardHit);
fprintf(fid,'TotalStandardHit/TotalHitRate %f\n',TotalStandardHit/TotalHitRate);
fclose(fid);
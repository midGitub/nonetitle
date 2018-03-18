clear,clc
%以下区域为每台机器不同的地方%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M4/M4LuckyPayout.txt','%d %s %s %d %f %f %d %s %s %s %s %s %f %s','headerlines',1);
%以下为变量命名
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
Reel4Wild=s12;
StandardHit=s13;
IsShortLucky=s14;
%以上区域为每台机器不同的地方%

NumOfItem=length(s1);%中奖组合条目数
TotalHitRate=sum(OverallHit);%总中奖率

%以下区域为每台机器不同的地方%
Reel4WildNum=zeros(NumOfItem,4);%卷轴4上的Wild概率分布表
WildMultipleInReel4=zeros(NumOfItem,1);%卷轴4上的Wild倍数的期望值
for i=1:NumOfItem
    string=strsplit(Reel4Wild{i},',');
    for j=1:4
        Reel4WildNum(i,j)=str2num(string{j});
    end
    WildMultipleInReel4(i)=Reel4WildNum(i,1)*3+Reel4WildNum(i,2)*7+Reel4WildNum(i,3)*20+Reel4WildNum(i,4)*50;
end
%以下区域为每台机器不同的地方%

Expectation=zeros(NumOfItem,1);%期望值的分布表
for i=1:12 
    %All3 & Any3 & Any2
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInReel4(i);
end
TotalExpectation=sum(Expectation);%总期望值
fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M4/M4LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectation %f\n',TotalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalHitRate);
fclose(fid);
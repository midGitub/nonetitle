using System.Collections;

//Common interface for PayoutDistData and NearHitDistData
public interface IJoyDistData
{
	int Id { get; set; }
	float Reward { get; set; }
	float OverallHit { get; set; }
}

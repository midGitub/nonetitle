using System.Collections;

//Common interface for PayoutData and NearHitData
public interface IJoyData
{
	int Id { get; set; }
	string[] Symbols { get; set; }
	PayoutType PayoutType { get; set; }
	int Count { get; set; }
	float OverallHit { get; set; }
	float[] Reel1Wild { get; set; }
	float[] Reel2Wild { get; set; }
	float[] Reel3Wild { get; set; }
	float[] Reel4Wild { get; set; }
	float Slide1 { get; set; }
	float Slide2 { get; set; }
	float Slide3 { get; set; }
}

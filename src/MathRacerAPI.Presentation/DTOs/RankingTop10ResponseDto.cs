namespace MathRacerAPI.Presentation.DTOs;

public class RankingTop10ResponseDto
{
    public List<PlayerRankingDto> Top10 { get; set; } = new();
    public int CurrentPlayerPosition { get; set; }
}

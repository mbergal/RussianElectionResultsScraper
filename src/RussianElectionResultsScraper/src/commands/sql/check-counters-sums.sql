select 
		SumVotingPlaceId, 
		SumCounter,
		SumValue,
		Id,
		VotingPlaceId,
		Counter,
		Value
	from 
	( select ParentId SumVotingPlaceId, Counter SumCounter, SUM( value ) SumValue from VotingResult vr inner join VotingPlace vp on vr.VotingPlaceId = vp.Id group by ParentId, Counter ) a 
	full join ( select vp.Id VotingPlaceId, vr.Id, Counter, value from VotingResult vr inner join VotingPlace vp on vr.VotingPlaceId = vp.Id where type <> 5 )  b on 
	a.SumVotingPlaceId = b.VotingPlaceId and a.SumCounter = b.Counter
	where isnull( SumValue, 0 ) <> isnull( Value, 0 )

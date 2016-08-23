

namespace std
{
	template <class TVal, class Pred> class WhereEnumerable;

	template <class TVal>
	class IEnumerator
	{
	public:
		virtual TVal GetCurrent()=0;
		virtual void MoveNext()=0;
		virtual bool HasNext()=0;
	};

	template <class TVal>
	class IEnumerable
	{
	public:
		virtual IEnumerator<TVal> GetEnumerator()=0;

		template <class Pred>
		WhereEnumerable<TVal, Pred> Where(Pred pred)
		{
			return WhereEnumerable<TVal, Pred>(GetEnumerator(), pred);
		}
	};
	
	template <class TVal, class Pred>
	class WhereEnumerator : public IEnumerator<TVal>
	{
		IEnumerator<TVal>* enu;
		Pred pred;
	public:
		virtual TVal GetCurrent() { return enu->GetCurrent(); }
		virtual void MoveNext()
		{
			enu->MoveNext();
			while (enu->HasNext() && !pred(enu->GetCurrent()))
				enu->MoveNext();
		}
		virtual bool HasNext() { return enu->HasNext(); }
	};
	template <class TVal, class Pred>
	class WhereEnumerable : public IEnumerable<TVal>
	{
		IEnumerator<TVal> begin;
		Pred pred;
	public:
		virtual IEnumerator<TVal> GetEnumerator() { return WhereEnumerator(&begin, pred); }
	};/**/
}
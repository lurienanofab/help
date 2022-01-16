using LNF.Help;
using LNF.Impl.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Help.Models
{
    public class FeesRepository
    {
        private readonly MongoRepository _repo = new MongoRepository("help");

        public FeesRepository()
        {
            _repo.MapClass<FeeSection>(x => x.Id);
        }

        public void AddSection(FeeSection section)
        {
            _repo.InsertOne("feeSections", section);
        }

        public IEnumerable<FeeSection> FindSections()
        {
            return _repo.Find<FeeSection>("feeSections");
        }

        public IEnumerable<FeeSection> FindSections(Expression<Func<FeeSection, bool>> expression)
        {
            return _repo.Find("feeSections", expression);
        }

        public FeeSection GetSection(Guid id)
        {
            return _repo.Get<FeeSection>("feeSections", id);
        }


        public void UpdateSection(Guid id, string title)
        {
            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>().Set(x => x.Title, title));
        }


        public void AddFeeLink(Guid id, FeeLink link)
        {
            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>().AddToSet(x => x.Links, link));
        }

        public void UpdateFeeLink(Guid id, int index, string text, string url)
        {
            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>()
                .Set(x => x.Links.ElementAt(index).Text, text)
                .Set(x => x.Links.ElementAt(index).Url, url));
        }

        public void DeleteSection(Guid id)
        {
            _repo.Delete<FeeSection>("feeSections", x => x.Id == id);
        }

        public void RemoveFeeLink(Guid id, FeeLink link)
        {
            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>().Pull(x => x.Links, link));
        }

        public void RemoveFeeLink(Guid id, int index)
        {
            string del = "delete_" + Guid.NewGuid().ToString("N");

            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>()
                .Set(x => x.Links.ElementAt(index).Text, del)
                .Set(x => x.Links.ElementAt(index).Url, del));

            _repo.Update("feeSections", x => x.Id == id, _repo.Updates<FeeSection>()
                .Pull(x => x.Links, new FeeLink { Text = del, Url = del }));
        }
    }
}
using OBGpgm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBGpgm.Interfaces
{
    public interface IMemberRepository
    {
        List<Member> SelectAll();
        List<Member> SelectAllPaidMembers();
        List<Member> SelectAlive();
        List<Member> SelectAllPhoto(bool isDeceased);
        Member SelectById(int id);
        Member SelectByEmail(String Email);
        Member SelectByOldest();
        Member SelectByYoungest();
        void Insert(Member mem);
        void Update(Member mem);
        void Delete(int id);
    }
}

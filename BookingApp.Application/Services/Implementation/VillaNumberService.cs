using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Services.Interface;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Hosting;

namespace BookingApp.Application.Services.Implementation
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckVillaNumberExists(int villa_number)
        {
            return _unitOfWork.VillaNumber.Any(u => u.Villa_Number == villa_number);
        }

        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Add(villaNumber);
            _unitOfWork.Save();
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                var villaToDelete = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == id);

                if (villaToDelete != null)
                {
                    _unitOfWork.VillaNumber.Remove(villaToDelete);
                    _unitOfWork.Save();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            } 
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbers()
        {
            return _unitOfWork.VillaNumber.GetAll(includeproperties: "Villa");
        }

        public VillaNumber GetVillaNumberById(int id)
        {
            return _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id, includeproperties: "Villa");
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Update(villaNumber);
            _unitOfWork.Save();
        }
    }
}

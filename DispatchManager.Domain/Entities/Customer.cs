using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { } // EF Constructor

    private Customer(string name, string email, string phone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Create(string name, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Se requiere el nombre del cliente");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new DomainException("Se requiere un correo electrónico de cliente válido");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Se requiere el teléfono del cliente");

        return new Customer(name.Trim(), email.Trim().ToLowerInvariant(), phone.Trim());
    }

    public void UpdateContactInfo(string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new DomainException("Se requiere un correo electrónico válido");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Se requiere teléfono");

        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

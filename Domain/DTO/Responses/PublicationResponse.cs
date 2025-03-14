using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class PublicationResponse
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public string Publisher { get; set; }
    public DepartmentResponse Department { get; set; }
    public CategoryResponse Category { get; set; }
    public string Status { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public List<AuthorResponse> Authors { get; set; }
    public decimal Progress { get; set; }
    public RoyaltyResponse Royalties { get; set; }
}

public class DepartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class AuthorResponse
{
    public string Name { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}

public class RoyaltyResponse
{
    public decimal Total { get; set; }
    public decimal Received { get; set; }
    public bool PendingPayment { get; set; }
} 
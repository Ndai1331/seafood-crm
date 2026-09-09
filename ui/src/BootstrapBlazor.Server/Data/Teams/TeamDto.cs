namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a team in the UI project.
    /// </summary>
    public class TeamDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the team.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new team - matches API specification
    /// </summary>
    public class CreateTeamDto
    {
        /// <summary>
        /// Code for the team (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing team - matches API specification
    /// </summary>
    public class UpdateTeamDto
    {
        /// <summary>
        /// Unique identifier of the team to update (required).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the team (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
    }
}


﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SarData.Common.Apis.Database.Training;

namespace SarData.Common.Apis.Database
{
  public interface IDatabaseApi
  {
    [Post("/trainingrecords")]
    Task<TrainingRecord> CreateTrainingRecord([Body] TrainingRecord trainingRecord);

    [Get("/members/{memberId}/requiredtraining")]
    Task<List<TrainingStatus>> ListMemberRequiredTraining(Guid memberId);
  }
}

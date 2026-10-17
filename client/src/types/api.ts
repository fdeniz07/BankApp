export type CreditApplicationStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'

export interface DashboardSummary {
  scope: 'Staff' | 'Customer' | string
  customerTotal?: number | null
  individualActive?: number | null
  corporateActive?: number | null
  customerInactive?: number | null
  creditTypeTotal?: number | null
  creditTypeIndividual?: number | null
  creditTypeCorporate?: number | null
  applicationTotal: number
  applicationPending: number
  applicationApproved: number
  applicationRejected: number
  applicationCancelled: number
}
export type CreditCategory = 'Individual' | 'Corporate'
export type CreditRiskLevel = 'Low' | 'Medium' | 'High'
export type LocaleCode = 'tr' | 'en' | 'de'

export interface AccessToken {
  token: string
  expiration: string
}

export interface LoggedResponse {
  accessToken: AccessToken
  displayName?: string | null
  customerCategory?: CreditCategory | null
}

export type Gender = 'Female' | 'Male' | 'PreferNotToSay'

export interface RegisterIndividualRequest {
  email: string
  password: string
  phoneNumber: string
  address: string
  firstName: string
  lastName: string
  nationalId: string
  dateOfBirth?: string | null
  gender: Gender
  motherName?: string | null
  fatherName?: string | null
}

export interface IndividualCustomerListItem {
  id: string
  customerNumber: string
  email: string
  phoneNumber: string
  firstName: string
  lastName: string
  nationalId: string
  isActive: boolean
  isDeleted: boolean
}

export interface IndividualCustomerDetail {
  id: string
  customerNumber: string
  email: string
  phoneNumber: string
  address: string
  firstName: string
  lastName: string
  nationalId: string
  dateOfBirth?: string | null
  gender?: string | null
  motherName?: string | null
  fatherName?: string | null
  isActive: boolean
  createdDate?: string
  updatedDate?: string | null
}

export interface CreateIndividualCustomerRequest {
  customerNumber: string
  email: string
  password: string
  phoneNumber: string
  address: string
  firstName: string
  lastName: string
  nationalId: string
  dateOfBirth?: string | null
  gender: Gender
  motherName?: string | null
  fatherName?: string | null
}

export interface CorporateCustomerListItem {
  id: string
  customerNumber: string
  email: string
  phoneNumber: string
  companyName: string
  taxNumber: string
  authorizedPersonName: string
  isActive: boolean
  isDeleted: boolean
}

export interface CorporateCustomerDetail {
  id: string
  customerNumber: string
  email: string
  phoneNumber: string
  address: string
  companyName: string
  taxNumber: string
  taxOffice: string
  companyRegistrationNumber: string
  companyEmail?: string | null
  authorizedPersonName: string
  companyAddress?: string | null
  companyType?: string | null
  companySector?: string | null
  companyIndustry?: string | null
  companySubIndustry?: string | null
  companyFoundationDate?: string | null
  isActive: boolean
  createdDate?: string
  updatedDate?: string | null
}

export interface CreateCorporateCustomerRequest {
  customerNumber: string
  email: string
  password: string
  phoneNumber: string
  address: string
  companyName: string
  taxNumber: string
  taxOffice: string
  companyRegistrationNumber: string
  companyEmail?: string | null
  authorizedPersonName: string
  companyAddress?: string | null
  companyType?: string | null
  companySector?: string | null
  companyIndustry?: string | null
  companySubIndustry?: string | null
  companyFoundationDate?: string | null
}

export interface GetListResponse<T> {
  from: number
  index: number
  size: number
  count: number
  pages: number
  items: T[]
  hasPrevious: boolean
  hasNext: boolean
}

export interface CreditApplicationListItem {
  id: string
  applicationNumber: string
  customerId: string
  creditSubTypeId: string
  creditSubTypeName: string
  currencyCode: string
  requestedAmount: number
  requestedTermMonths: number
  interestRate: number
  approvedInterestRate?: number | null
  status: CreditApplicationStatus
  applicationDate: string
  isDeleted: boolean
}

export interface CreditApplicationDetail extends CreditApplicationListItem {
  customerName: string
  customerCategory?: CreditCategory | null
  monthlyIncome?: number | null
  existingDebtAmount?: number | null
  purpose?: string | null
  decisionDate?: string | null
  approvedAmount?: number | null
  approvedTermMonths?: number | null
  approvedInterestRate?: number | null
  rejectionReason?: string | null
  notes?: string | null
  decisionBy?: string | null
  createdDate: string
  updatedDate?: string | null
}

export interface CreditTypeListItem {
  id: string
  name: string
  code: string
  locale: LocaleCode | string
  category: CreditCategory
  currencyCode: string
  riskLevel: CreditRiskLevel
  isActive: boolean
  isDeleted: boolean
}

export interface CreateCreditTypeRequest {
  name: string
  code: string
  locale: LocaleCode | string
  category: CreditCategory
  description?: string | null
  currencyCode: string
  riskLevel: CreditRiskLevel
  requiresCollateral: boolean
  requiresGuarantor: boolean
}

export interface SetCreditTypeActiveResponse {
  id: string
  name: string
  code: string
  isActive: boolean
}

export interface CreditSubTypeListItem {
  id: string
  creditTypeId: string
  name: string
  code: string
  locale: LocaleCode | string
  category: CreditCategory | string
  currencyCode: string
  minAmount: number
  maxAmount: number
  interestRate: number
  minTermMonths: number
  maxTermMonths: number
  amountOptions: number[]
  termOptions: number[]
  isActive: boolean
  isDeleted: boolean
}

export interface PaymentPlanInstallment {
  month: number
  payment: number
  principal: number
  interest: number
  remainingBalance: number
}

export interface PaymentPlanResponse {
  amount: number
  termMonths: number
  interestRate: number
  currencyCode: string
  monthlyPayment: number
  totalPayment: number
  totalInterest: number
  installments: PaymentPlanInstallment[]
}

export interface CreateApplicationRequest {
  customerId?: string
  creditSubTypeId: string
  requestedAmount: number
  requestedTermMonths: number
  monthlyIncome?: number | null
  existingDebtAmount?: number | null
  purpose?: string | null
  notes?: string | null
}

export type PersonTitle = 'Dr' | 'Prof'

export type StaffPosition =
  | 'CreditAnalyst'
  | 'RelationshipManager'
  | 'BranchOfficer'
  | 'RiskOfficer'
  | 'OperationsSpecialist'
  | 'BranchManager'

export interface StaffUserListItem {
  id: string
  email: string
  isActive: boolean
  isManager: boolean
  firstName?: string | null
  lastName?: string | null
  title?: PersonTitle | null
  gender?: Gender | null
  dateOfBirth?: string | null
  position?: StaffPosition | null
  profileImageUrl?: string | null
  roles: string[]
  createdDate: string
}

export interface CreateStaffUserRequest {
  email: string
  password: string
  isManager: boolean
  firstName?: string | null
  lastName?: string | null
  title?: PersonTitle | null
  gender?: Gender | null
  dateOfBirth?: string | null
  position?: StaffPosition | null
}

export interface UpdateStaffUserRequest {
  id: string
  email: string
  newPassword?: string | null
  isActive: boolean
  isManager: boolean
  firstName?: string | null
  lastName?: string | null
  title?: PersonTitle | null
  gender?: Gender | null
  dateOfBirth?: string | null
  position?: StaffPosition | null
}

export interface MyProfile {
  id: string
  email: string
  firstName?: string | null
  lastName?: string | null
  title?: PersonTitle | null
  gender?: Gender | null
  dateOfBirth?: string | null
  position?: StaffPosition | null
  profileImageUrl?: string | null
  isActive: boolean
  isStaff: boolean
  roles: string[]
}

export interface UploadProfileImageResponse {
  userId: string
  profileImageUrl: string
}

export interface ApproveRequest {
  approvedAmount: number
  approvedTermMonths: number
  approvedInterestRate: number
  decisionBy?: string
  notes?: string
}

export interface RejectRequest {
  rejectionReason: string
  decisionBy?: string
  notes?: string
}

export type ApplicationSortBy =
  | 'applicationNumber'
  | 'requestedAmount'
  | 'requestedTermMonths'
  | 'status'
  | 'applicationDate'
  | 'creditSubTypeName'
